using System.IO;
using Microsoft.Win32;

namespace TextReader.Services;

public static class FileService
{
    public static string LoadText(string path)
    {
        return File.ReadAllText(path);
    }

    public static void SaveText(string path, string text)
    {
        File.WriteAllText(path, text);
    }

    public static string? SaveTextAs(string text)
    {
        SaveFileDialog dialog = new();

        dialog.Filter = "Text Files (*.txt)|*.txt";

        if (dialog.ShowDialog() != true)
            return null;

        File.WriteAllText(dialog.FileName, text);

        return dialog.FileName;
    }
}