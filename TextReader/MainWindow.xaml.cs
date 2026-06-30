using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using TextReader.Models;
using TextReader.ReadingEngine;
using TextReader.Services;
using WinColor = System.Drawing.Color;

namespace TextReader;

public partial class MainWindow : Window
{
    private readonly EditorState _state = new();
    private readonly SpeechService _speech = new();
    private readonly BookLibraryService _library = new();

    private BookmarkService _bookmarkService = new();
    private BookInfo _currentBook;
    private ReadingSession _readingSession;


    private SpeechHighlightService _highlightService;

    private volatile bool _isStopping;
    private bool _isTtsRunning;

    private bool _isSettingsApplying;
    private string _currentWord;
    private string _currentTranslation;
    private AppData _appData;
    private NotebookService _notebookService;

    private int _lastOffset;
    private bool _hasPendingHighlight;
    private DispatcherTimer _highlightTimer;
    private bool _isSpeakingPage;
    private int _ttsResumePageIndex = -1;
    private bool _isPaused;

    private AppMode _currentMode = AppMode.Edit;

    public enum AppMode
    {
        Edit,
        ReadBook,
        ReadScroll,
        Notebook
    }
    public MainWindow()
    {
        InitializeComponent();
        _appData = SaveService.Load();

        ThemeService.ApplyTheme(_appData.Theme);

        CurrentThemeName = _appData.Theme;

        if (!string.IsNullOrWhiteSpace(_appData.VoiceName))
            _speech.SetVoice(_appData.VoiceName);

        _speech.SetRate(_appData.SpeechRate);
        _speech.ProgressChanged += OnSpeechProgress;
        _speech.SpeakCompleted += OnSpeakCompleted;

        Loaded += (_, __) => Dispatcher.BeginInvoke(InitHighlight);

        _highlightTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(40)
        };

        _highlightTimer.Tick += (s, e) =>
        {
            if (!_hasPendingHighlight || _highlightService == null || Reader.Document == null)
                return;

            _hasPendingHighlight = false;

            var pointer = FlowDocumentNavigator.GetPointerAtOffset(
                Reader.Document,
                _lastOffset);

            if (pointer == null)
                return;

            _highlightService.Highlight(pointer);
        };

        _highlightTimer.Start();

        foreach (var item in _appData.Vocabulary)
        {
            VocabularyBook.Items.Add(item);
        }


        Editor.TextChanged += Editor_TextChanged;
        Editor.SelectionChanged += Editor_SelectionChanged;
        Reader.PreviewMouseUp += Reader_PreviewMouseUp;
        NotebookList.ItemsSource = VocabularyBook.Items;
        _notebookService = new NotebookService();

        LoadFonts();
        LoadFontSizes();
        UpdateModeButton();
    }
    public void UpdateModeButton()
    {
        ModeButton.Content =
            (_currentMode != AppMode.Edit
                ? System.Windows.Application.Current.FindResource("EditMode")
                : System.Windows.Application.Current.FindResource("ReadMode"));
    }
    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, Editor))
            return;

        Editor.Document = new FlowDocument();

        _state.IsModified = false;
        _state.CurrentFilePath = null;

        EditorStateService.UpdateTitle(this, _state);
    }
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, Editor))
            return;

        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = "Все файлы (*.*)|*.*|Текстовые файлы (*.txt)|*.txt|Документы Word (*.docx)|*.docx"
        };

        if (dialog.ShowDialog() != true)
            return;

        _state.IsLoading = true;

        string extension = Path.GetExtension(dialog.FileName);

        if (extension != ".txt" && extension != ".docx")
        {
            MessageBox.Show("Неподдерживаемый формат файла.");
            _state.IsLoading = false;
            return;
        }

        if (extension == ".docx")
        {
            BookInfo book = _library.ImportBook(dialog.FileName);

            _currentBook = book;

            Editor.Document =
                FileService.LoadDocxAsFlowDocument(book.FilePath);

            SetMode(AppMode.ReadScroll);

        }
        else if (extension == ".txt")
        {
            string text = File.ReadAllText(dialog.FileName);
            Editor.Document = new FlowDocument(new Paragraph(new Run(text)));

            SetMode(AppMode.Edit); 
        }

        _state.IsLoading = false;
        _state.CurrentFilePath = dialog.FileName;
        _state.IsModified = false;

        EditorStateService.UpdateTitle(this, _state);
        UpdateModeButton();
        UpdatePageUI();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog();

        dialog.Filter = "Text Files (*.txt)|*.txt|Word Documents (*.docx)|*.docx";

        if (dialog.ShowDialog() != true) return;

        string path = dialog.FileName;
        string ext = Path.GetExtension(path);
        string text = GetText();

        if (ext == ".txt")
        {
            FileService.SaveText(path, text);
        }
        else if (ext == ".docx")
        {
            FileService.SaveDocxFromRichTextBox(path, Editor);
        }
            _state.CurrentFilePath = path;
            _state.IsModified = false;
            _state.IsLoading = false;
            EditorStateService.UpdateTitle(this, _state);

    }

    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.ToggleBold(Editor);
    }

    private void Italic_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.ToggleItalic(Editor);
    }

    private void Underline_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.ToggleUnderline(Editor);
    }

    private void Strike_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.ToggleStrike(Editor);
    }

    private void AlignLeft_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.AlignLeft(Editor);
    }

    private void AlignCenter_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.AlignCenter(Editor);
    }

    private void AlignRight_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.AlignRight(Editor);
    }

    private void Justify_Click(object sender, RoutedEventArgs e)
    {
        TextFormatService.Justify(Editor);
    }


    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_state.IsLoading)
            return;

        _state.IsModified = true;

        EditorStateService.UpdateTitle(this, _state);
    }

    private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        string selectedText = Editor.Selection.Text;

        if (string.IsNullOrWhiteSpace(selectedText))
        {
            TranslatePopup.IsOpen = false;
            return;
        }

        var word = selectedText.Trim().ToLower();

        string translation = DictionaryService.GetTranslation(word);

        if (!string.IsNullOrWhiteSpace(translation))
        {
            PopupText.Text = translation;
            TranslatePopup.IsOpen = true;
        }
        else
        {
            TranslatePopup.IsOpen = false;
        }
    }




    #region FONTS 
    //FONTS
    private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
    {
        ApplyFontSize();
    }
    private void FontSize_LostFocus(object sender, RoutedEventArgs e)
    {
        ApplyFontSize();
    }
    private void FontSize_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ApplyFontSize();
    }

    private void ApplyFontSize()
    {
        if (double.TryParse(FontSizeBox.Text, out double size) && size > 0)
        {
            Editor.Selection.ApplyPropertyValue(
                TextElement.FontSizeProperty,
                size);
        }
    }
    private void LoadFontSizes()
    {
        int[] sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72 };

        foreach (var size in sizes)
        {
            FontSizeBox.Items.Add(size.ToString());
        }
    }

    private void FontFamily_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (FontFamilyBox.SelectedItem is string fontName)
        {
            Editor.Selection.ApplyPropertyValue(
                TextElement.FontFamilyProperty,
                new FontFamily(fontName));
        }
    }
    private void ColorPicker_Changed(
    object sender,
    RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (e.NewValue.HasValue)
        {
            Editor.Selection.ApplyPropertyValue(
                TextElement.ForegroundProperty,
                new SolidColorBrush(e.NewValue.Value));
        }
    }


    private void LoadFonts()
    {

        foreach (var font in Fonts.SystemFontFamilies)
        {
            FontFamilyBox.Items.Add(font.Source);
        }
        FontFamilyBox.SelectedIndex = 0;
    }

    #endregion
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, Editor))
        {
            e.Cancel = true;
            return;
        }

        _speech?.Stop();
        Thread.Sleep(50);
        _speech?.Dispose();

        Editor.TextChanged -= Editor_TextChanged;
        Editor.SelectionChanged -= Editor_SelectionChanged;

        _speech.ProgressChanged -= OnSpeechProgress;

        _highlightService?.Dispose(); 


        _appData.Theme = CurrentThemeName;
        _appData.Vocabulary = VocabularyBook.Items.ToList();

        SaveService.Save(_appData);
    }
    

    private string CurrentThemeName = "Light";

    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
        _isSettingsApplying = true;

        SettingsWindow settings = new()
        {
            SelectedTheme = CurrentThemeName,
            SelectedVoice = _appData.VoiceName,
            SelectedRate = _appData.SpeechRate
        };

        _speech.Stop();



        if (settings.ShowDialog() == true)
        {
            CurrentThemeName = settings.SelectedTheme;

            _speech.SetVoice(settings.SelectedVoice);
            _speech.SetRate(settings.SelectedRate);

            await RestartSpeechAsync();

            _appData.VoiceName = settings.SelectedVoice;
            _appData.SpeechRate = settings.SelectedRate;

            SaveService.Save(_appData);
        }

        _isSettingsApplying = false;
    }

    private string GetText()
    {
        return new TextRange(
            Editor.Document.ContentStart,
            Editor.Document.ContentEnd
        ).Text;
    }


    #region ReadingMode


    private void SetMode(AppMode mode)
    {
        _highlightService?.Dispose();
        _highlightService = null;

        _currentMode = mode;

        switch (mode)
        {
            case AppMode.Edit:
                _highlightService?.Dispose();
                _highlightService = null;

                Reader.Document = new FlowDocument();

                ReaderContainer.Visibility = Visibility.Collapsed;
                ReadingToolbar.Visibility = Visibility.Collapsed;

                Editor.Visibility = Visibility.Visible;
                break;

            case AppMode.ReadBook:
                SwitchToReader();
                SetBookMode();
                break;

            case AppMode.ReadScroll:
                SwitchToReader();
                SetScrollMode();
                break;
        }
    }
    private void SetBookMode()
    {
        Reader.Margin = new Thickness(300, 20, 300, 20);
    }

    private void SetScrollMode()
    {
        Reader.Margin = new Thickness(0);
    }


    private void SwitchToReader()
    {

        _readingSession = new ReadingSession();

        var readingDoc =
            ReadingDocumentBuilder.Build(Editor.Document);

        _readingSession.Open(readingDoc);

        Reader.Document = _readingSession.GetCurrentPage();

        Editor.Visibility = Visibility.Collapsed;
        NotebookList.Visibility = Visibility.Collapsed;

        ReaderContainer.Visibility = Visibility.Visible;
        ReadingToolbar.Visibility = Visibility.Visible;

        Reader.IsReadOnly = true;

        Grid.SetColumn(Reader, 0);
        Grid.SetColumnSpan(Reader, 2);

        Reader.Document.PageWidth = double.NaN;
        Reader.UpdateLayout();

        _highlightService?.Dispose();
        _highlightService = new SpeechHighlightService(Reader);
        _highlightService.Init(Reader);

        var reading = ReadingDocumentBuilder.Build(Editor.Document);
        var manager = new PageManager(reading);

        var page = manager.GetCurrentPage();

        Reader.Document = PageRenderer.Render(page);
    }

    private void ModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMode != AppMode.Edit)
        {
            SetMode(AppMode.Edit);
        }
        else
        {
            SetMode(AppMode.ReadScroll);
        }

        UpdateModeButton();
    }

    private void ReadBook_Click(object sender, RoutedEventArgs e)
    {
        SetMode(AppMode.ReadBook);
    }

    private void ReadScroll_Click(object sender, RoutedEventArgs e)
    {
        SetMode(AppMode.ReadScroll);
    }

    #region TEXT_TO_SPEECH
    private void TextToSpeech_Click(object sender, RoutedEventArgs e)
    {
        if (_isSettingsApplying) return;

        if (_isTtsRunning)
            return;

        _isTtsRunning = true;
        _isSpeakingPage = true;

        StartSpeakingCurrentPage();
    }

    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _speech.Pause();
        _isPaused = true;

        _ttsResumePageIndex = _readingSession.CurrentPageIndex;
    }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        if (_readingSession == null)
            return;

        _speech.Resume();
        _isPaused = false;

        if (_ttsResumePageIndex >= 0 &&
            _ttsResumePageIndex != _readingSession.CurrentPageIndex)
        {
            _readingSession.GoToPage(_ttsResumePageIndex);

            Reader.Document = _readingSession.GetCurrentPage();
            UpdatePageUI();
        }
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _isSpeakingPage = false;
        _isTtsRunning = false;

        _speech.Stop();

        _highlightService?.Clear();
    }

    private async Task RestartSpeechAsync()
    {
        if (_isSettingsApplying) return;

        string text = GetCleanText(Reader.Document);
        await Task.Run(() =>
        {
            _speech.Stop();
            Thread.Sleep(50);
        });

        _speech.Read(text);
    }

    private void OnSpeechProgress(int offset)
    {
        if (_isStopping || _highlightService == null || Reader.Document == null)
            return;

        _lastOffset = offset;
        _hasPendingHighlight = true;
    }

    private string GetCleanText(FlowDocument document)
    {
        string raw = new TextRange(document.ContentStart, document.ContentEnd).Text;
        // Удаляем все варианты переносов строк
        return raw.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
    }

    private void InitHighlight()
    {
        _highlightService = new SpeechHighlightService(Reader);
        _highlightService.Init(Reader);
    }

    #endregion

    private void Reader_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isSettingsApplying) return;

        var selection = Reader.Selection;

        if (selection == null)
        {
            TranslatePopup.IsOpen = false;
            return;
        }

        string selectedText = selection.Text?.Trim();

        if (string.IsNullOrWhiteSpace(selectedText))
        {
            TranslatePopup.IsOpen = false;
            return;
        }

        selectedText = selectedText.ToLower();

        string translation = DictionaryService.GetTranslation(selectedText);

        if (string.IsNullOrWhiteSpace(translation))
        {
            TranslatePopup.IsOpen = false;
            return;
        }

        _currentWord = selectedText;
        _currentTranslation = translation;

        PopupText.Text = translation;
        TranslatePopup.IsOpen = true;
    }


    private void AddToNotebook_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentWord))
            return;

        VocabularyBook.Add(_currentWord, _currentTranslation);

        MessageBox.Show(System.Windows.Application.Current.FindResource("AddedToNotebook").ToString());
    }

    private void AddBookmark_Click(object sender, RoutedEventArgs e)
    {
        if (_currentBook == null)
            return;

        Bookmark bookmark = new Bookmark
        {
            BookId = _currentBook.Id,
            Name = _currentWord,
            Page = _readingSession.CurrentPageIndex
        };

        _bookmarkService.Add(bookmark);

        LoadBookmarks();

        TranslatePopup.IsOpen = false;

        MessageBox.Show("Закладка добавлена.");
    }

    private bool _isNotebookOpen = false;

    private void ToggleNotebook_Click(object sender, RoutedEventArgs e)
    {
        if (_isNotebookOpen)
        {
            NotebookList.Visibility = Visibility.Collapsed;
            Editor.Visibility = Visibility.Visible;
        }
        else
        {
            ShowNotebook();
        }

        _isNotebookOpen = !_isNotebookOpen;
    }

    public void ShowNotebook()
        {
            Editor.Visibility = Visibility.Collapsed;
        ReaderContainer.Visibility = Visibility.Collapsed;
        ReadingToolbar.Visibility = Visibility.Collapsed;


        NotebookList.Visibility = Visibility.Visible;
            NotebookList.ItemsSource = VocabularyBook.Items;
        }

    private void NotebookList_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
    {
        if (NotebookList.SelectedItem is not VocabularyItem item)
            return;

        var result = MessageBox.Show(
            (System.Windows.Application.Current.FindResource("DeleteQuestion").ToString()),
            System.Windows.Application.Current.FindResource("Delete").ToString(),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            VocabularyBook.Items.Remove(item);
        }

        NotebookList.SelectedItem = null;
    }

    private void StartTraining_Click(object sender, RoutedEventArgs e)
    {
        _notebookService.StartTraining();
        var trainingWindow = new TrainingWindow(_notebookService);
        trainingWindow.Owner = this;

        this.Hide(); 

        trainingWindow.ShowDialog();

        this.Show();
    }
    #endregion

    #region NEWSYSTEM


    private void OnSpeakCompleted()
    {
        Dispatcher.Invoke(() =>
        {
            if (_readingSession == null)
                return;

            if (!_isTtsRunning)
                return;

            if (_readingSession.NextPage())
            {
                Reader.Document = _readingSession.GetCurrentPage();

                UpdatePageUI();

                StartSpeakingCurrentPage();
            }
            else
            {
                _isSpeakingPage = false;
                _isTtsRunning = false;
            }
        });
    }

    private void StartSpeakingCurrentPage()
    {
        if (_readingSession == null)
            return;

        if (!_isTtsRunning)
            return;

        _isTtsRunning = true;
        _ttsResumePageIndex = _readingSession.CurrentPageIndex;

        string text = GetCleanText(Reader.Document);

        _speech.Read(text);
    }

    private void UpdatePageUI()
    {
        if (_readingSession == null) return;

        PageIndicator.Text =
            $"{_readingSession.CurrentPageIndex + 1} / {_readingSession.TotalPages}";
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_readingSession?.NextPage() == true)
        {
            Reader.Document = _readingSession.GetCurrentPage();
            UpdatePageUI();
        }
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (_readingSession?.PreviousPage() == true)
        {
            Reader.Document = _readingSession.GetCurrentPage();
            UpdatePageUI();
        }
    }

    private void GoToPage_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PageBox.Text, out int page))
        {
            if (_readingSession.GoToPage(page - 1))
            {
                Reader.Document = _readingSession.GetCurrentPage();
                UpdatePageUI();
            }
        }
    }

    #endregion

    #region BOOKMARKS

    private bool _bookmarksOpen = false;

    private void ToggleBookmarksPanel(object sender, RoutedEventArgs e)
    {
        ToggleBookmarksPanelLogic();
    }
    private void ToggleBookmarksPanelLogic()
    {
        if (_bookmarksOpen)
            BookmarksColumn.Width = new GridLength(0);
        else
        {
            BookmarksColumn.Width = new GridLength(250);
            LoadBookmarks();
        }

        _bookmarksOpen = !_bookmarksOpen;
    }

    private void LoadBookmarks()
    {
        if (_currentBook == null)
            return;

        var bookmarks = _bookmarkService
            .GetBookmarks(_currentBook.Id);

        BookmarksList.ItemsSource = bookmarks;
    }

    private void Bookmark_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (BookmarksList.SelectedItem is not Bookmark bm)
            return;

        _readingSession.GoToPage(bm.Page);

        Reader.Document = _readingSession.GetCurrentPage();

        UpdatePageUI();

        HighlightBookmark(bm);
    }

    private void DeleteBookmark_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem &&
            menuItem.DataContext is Bookmark bookmark)
            {
                var result = MessageBox.Show(
                "Удалить закладку?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

            _bookmarkService.Remove(bookmark.Id);
            LoadBookmarks();
        }
    }

    private void HighlightBookmark(Bookmark bm)
    {
        if (Reader.Document == null)
            return;

        var pointer = FlowDocumentNavigator.FindWordPointer(Reader.Document, bm.Name);

        if (pointer == null)
            return;

        _highlightService?.Clear(); 
        _highlightService.Highlight(pointer);
    }

    #endregion

}