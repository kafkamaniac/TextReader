using Microsoft.Win32;
using System.IO;
using System.Windows;
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

    public static bool AskToSave(EditorState state, string text)
    {
        if (!state.IsModified)
            return true;

        MessageBoxResult result = MessageBox.Show(
            "Сохранить изменения?",
            "TextReader",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        switch (result)
        {
            case MessageBoxResult.Yes:

                string? path;


                if (state.CurrentFilePath != null)
                {
                    SaveCurrentFile(state, text);
                    return true;
                }

                SaveFileDialog dialog = new();
                dialog.Filter = "Текстовые файлы (*.txt)|*.txt|Документы Word (*.docx)|*.docx|Все файлы (*.*)|*.*";
                if (dialog.ShowDialog() != true)
                    return false;

                path = dialog.FileName;
                string newExt = Path.GetExtension(path).ToLower();

                if (newExt == ".docx")
                {
                    FileService.SaveDocx(path, text);
                }
                else if (newExt == ".txt")
                {
                    FileService.SaveText(path, text);
                }
                else
                {
                    MessageBox.Show("Неподдерживаемый формат файла.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;

            case MessageBoxResult.No:
                return true;

            default:
                return false;
        }
    }

    private static bool SaveCurrentFile(EditorState state, string text)
    {
        if (state.CurrentFilePath == null)
            return false;

        string ext = Path.GetExtension(state.CurrentFilePath).ToLower();

        if (ext == ".docx")
        {
            FileService.SaveDocx(state.CurrentFilePath, text);
        }
        else if (ext == ".txt")
        {
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