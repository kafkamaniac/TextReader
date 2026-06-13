using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TextReader.Models;
using TextReader.Services;
using System.ComponentModel;

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
        if (!EditorStateService.AskToSave(_state, Editor.Text))
            return;

        Editor.Clear();
        
        _state.IsModified = false;
        _state.CurrentFilePath = null;

        EditorStateService.UpdateTitle(this, _state);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state, Editor.Text))
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
                Editor.Text = FileService.LoadDocx(dialog.FileName);
            }
            else if (extension == ".txt")
            {
                Editor.Text = File.ReadAllText(dialog.FileName);
            }

            _state.CurrentFilePath = dialog.FileName;

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
        if (ext == ".txt")
        {
            FileService.SaveText(path, Editor.Text);
        }
        else if (ext == ".docx")
        {
            FileService.SaveDocx(path, Editor.Text);
        }
            _state.CurrentFilePath = path;
            _state.IsModified = false;
            _state.IsLoading = false;
            EditorStateService.UpdateTitle(this, _state);

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
        if (!EditorStateService.AskToSave(_state, Editor.Text))
        {
            e.Cancel = true;
        }
    }


}