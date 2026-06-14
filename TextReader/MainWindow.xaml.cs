using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TextReader.Models;
using TextReader.Services;

namespace TextReader;

public partial class MainWindow : Window
{
    private readonly EditorState _state = new();
    public MainWindow()
    {
        InitializeComponent();
        Editor.TextChanged += Editor_TextChanged;

        LoadFonts();
        LoadFontSizes();
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

        OpenFileDialog dialog = new OpenFileDialog();

        dialog.Filter =
    "Все файлы (*.*)|*.*|Текстовые файлы (*.txt)|*.txt|Документы Word (*.docx)|*.docx";

        if (dialog.ShowDialog() == true)
        {
            _state.IsLoading = true;

            string extension = Path.GetExtension(dialog.FileName);
            if (extension != ".txt" && extension != ".docx")
            {
                MessageBox.Show("Неподдерживаемый формат файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _state.IsLoading = false;
                return;
            }
            else if (extension == ".docx")
            {
                Editor.Document = FileService.LoadDocxAsFlowDocument(dialog.FileName);
            }
            else if (extension == ".txt")
            {
                string text = File.ReadAllText(dialog.FileName);
                Editor.Document = new FlowDocument(new Paragraph(new Run(text)));
            }

            _state.IsLoading = false;
            _state.CurrentFilePath = dialog.FileName;
            _state.IsModified = false;

            EditorStateService.UpdateTitle(this, _state);

        }

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

    private void LoadFonts()
    {
        foreach (var font in Fonts.SystemFontFamilies)
        {
            FontFamilyBox.Items.Add(font.Source);
        }
    }

    #endregion
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, Editor))
        {
            e.Cancel = true;
        }
    }

    private string GetText()
    {
        return new TextRange(
            Editor.Document.ContentStart,
            Editor.Document.ContentEnd
        ).Text;
    }
}