using System.IO;

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
}