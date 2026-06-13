using System.IO;
using System.Windows;
using TextReader.Models;

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

    public static bool AskToSave(EditorState state)
    {
        if (!state.IsModified)
            return true;

        MessageBoxResult result = MessageBox.Show(
            "Сохранить изменения?",
            "TextReader",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        return result switch
        {
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => true,
            _ => false
        };
    }
}