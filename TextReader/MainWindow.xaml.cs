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
using TextReader.Models;
using TextReader.Services;
using WinColor = System.Drawing.Color;

namespace TextReader;

public partial class MainWindow : Window
{
    private readonly EditorState _state = new();
    private readonly SpeechService _speech = new();

    private string _currentWord;
    private string _currentTranslation;
    private AppData _appData;
    private NotebookService _notebookService;

    private AppMode _currentMode = AppMode.Edit;
    public enum AppMode
    {
        Edit,
        ReadPages,
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
            Editor.Document = FileService.LoadDocxAsFlowDocument(dialog.FileName);

            SetMode(AppMode.ReadPages); 

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

        _appData.Theme = CurrentThemeName;
        _appData.Vocabulary = VocabularyBook.Items.ToList();

        SaveService.Save(_appData);
    }
    

    private string CurrentThemeName = "Light";

    private async void Settings_Click(object sender, RoutedEventArgs e)
    {
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
        _currentMode = mode;

        switch (mode)
        {
            case AppMode.Edit:
                Reader.Visibility = Visibility.Collapsed;
                ReadingToolbar.Visibility = Visibility.Collapsed;

                Editor.Visibility = Visibility.Visible;
                break;

            case AppMode.ReadPages:
                SwitchToReader();
                break;

            case AppMode.ReadBook:
                SwitchToReader();
                break;

            case AppMode.ReadScroll:
                SwitchToReader();
                break;
        }
    }

    private void SwitchToReader()
    {

        UpdateModeButton();

        Reader.Document = CloneFlowDocument(Editor.Document);

        Editor.Visibility = Visibility.Collapsed;
        Reader.Visibility = Visibility.Visible;
        ReadingToolbar.Visibility = Visibility.Visible;
    }

    private FlowDocument? CloneFlowDocument(FlowDocument original)
    {
        if (original == null) return null;

        string xaml = XamlWriter.Save(original);

        using var stringReader = new StringReader(xaml);
        using var xmlReader = System.Xml.XmlReader.Create(stringReader);

        return (FlowDocument)XamlReader.Load(xmlReader);
    }

    private void ModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMode != AppMode.Edit)
        {
            SetMode(AppMode.Edit);
        }
        else
        {
            SetMode(AppMode.ReadPages);
        }

        UpdateModeButton();
    }

    private void ReadPages_Click(object sender, RoutedEventArgs e)
    {
        SetMode(AppMode.ReadPages);
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
        string text = new TextRange(
            Reader.Document.ContentStart,
            Reader.Document.ContentEnd).Text;

        _speech.Read(text);
    }
    private void Pause_Click(object sender, RoutedEventArgs e)
    {
        _speech.Pause();
    }

    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        _speech.Resume();
    }

    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        _speech.Stop();
    }

    private async Task RestartSpeechAsync()
    {
        string text = new TextRange(
            Reader.Document.ContentStart,
            Reader.Document.ContentEnd).Text;

        await Task.Run(() =>
        {
            _speech.Stop();
            Thread.Sleep(50);
        });

        _speech.Read(text);
    }


    #endregion

    private void Reader_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
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
            Reader.Visibility = Visibility.Collapsed;

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


}