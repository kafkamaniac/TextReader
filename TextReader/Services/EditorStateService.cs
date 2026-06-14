using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using TextReader.Models;
using static System.Net.Mime.MediaTypeNames;

namespace TextReader.Services;

public static class EditorStateService
{
    public static void UpdateTitle(Window window, EditorState state)
    {
        string fileName = state.CurrentFilePath == null
            ? "Безымянный"
            : Path.GetFileName(state.CurrentFilePath);

        window.Title = state.IsModified
            ? $"{fileName} *"
            : fileName;
    }

    public static bool AskToSave(EditorState state, RichTextBox editor)
    {
        if (!state.IsModified)
            return true;

        var result = MessageBox.Show(
            "Сохранить изменения?",
            "TextReader",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        switch (result)
        {
            case MessageBoxResult.Yes:

                if (state.CurrentFilePath != null)
                {
                    return SaveCurrentFile(state, editor);
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Text (*.txt)|*.txt|Word (*.docx)|*.docx"
                };

                if (dialog.ShowDialog() != true)
                    return false;

                state.CurrentFilePath = dialog.FileName;

                return SaveCurrentFile(state, editor);

            case MessageBoxResult.No:
                return true;

            default:
                return false;
        }
    }

    private static bool SaveCurrentFile(EditorState state, RichTextBox editor)
    {
        if (state.CurrentFilePath == null)
            return false;

        string ext = Path.GetExtension(state.CurrentFilePath).ToLower();

        if (ext == ".docx")
        {
            FileService.SaveDocxFromRichTextBox(state.CurrentFilePath, editor);
        }
        else if (ext == ".txt")
        {
            string text = new TextRange(
                editor.Document.ContentStart,
                editor.Document.ContentEnd).Text;

            FileService.SaveText(state.CurrentFilePath, text);
        }
        else
        {
            MessageBox.Show("Неподдерживаемый формат файла.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        state.IsModified = false;
        return true;
    }
}