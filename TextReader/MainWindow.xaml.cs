using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, GetText()))
            return;

        Editor.Document = new FlowDocument();

        _state.IsModified = false;
        _state.CurrentFilePath = null;

        EditorStateService.UpdateTitle(this, _state);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {

        if (!EditorStateService.AskToSave(_state, GetText()))
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


    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_state.IsLoading)
            return;

        _state.IsModified = true;

        EditorStateService.UpdateTitle(this, _state);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, GetText()))
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