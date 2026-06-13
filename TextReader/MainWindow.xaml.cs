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
        if (!EditorStateService.AskToSave(_state))
            return;

        Editor.Clear();
        
        _state.IsModified = false;
        _state.CurrentFilePath = null;

        EditorStateService.UpdateTitle(this, _state);
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (!EditorStateService.AskToSave(_state))
            return;

        OpenFileDialog dialog = new OpenFileDialog();

        dialog.Filter = "Text Files (*.txt)|*.txt";

        if (dialog.ShowDialog() == true)
        {
            _state.IsLoading = true;
            Editor.Text = File.ReadAllText(dialog.FileName);
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

        dialog.Filter = "Text Files (*.txt)|*.txt";

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, Editor.Text);
            _state.CurrentFilePath = dialog.FileName;
            _state.IsModified = false;
            _state.IsLoading = false;
        }
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
        if (!EditorStateService.AskToSave(_state))
        {
            e.Cancel = true;
        }
    }


}