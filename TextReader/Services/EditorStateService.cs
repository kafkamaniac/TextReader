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
                string? path = FileService.SaveTextAs(text);
                if (path != null)
                {
                    state.CurrentFilePath = path;
                    state.IsModified = false;
                }

                return true;

            case MessageBoxResult.No:
                return true;

            default:
                return false;
        }
    }
}