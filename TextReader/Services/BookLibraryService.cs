using System.IO;
using System.Text.Json;
using TextReader.Models;
using System.Security.Cryptography;

namespace TextReader.Services;

public class BookLibraryService
{
    private readonly string _libraryFolder;
    private readonly string _libraryFile;

    private List<BookInfo> _books = new();

    public IReadOnlyList<BookInfo> Books => _books;

    public BookLibraryService()
    {
        string appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextReader");

        _libraryFolder = Path.Combine(appFolder, "Library");

        _libraryFile = Path.Combine(_libraryFolder, "library.json");

        Directory.CreateDirectory(_libraryFolder);

        LoadLibrary();
    }

    private void SaveLibrary()
    {
        string json = JsonSerializer.Serialize(
            _books,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_libraryFile, json);
    }

    private void LoadLibrary()
    {
        if (!File.Exists(_libraryFile))
            return;

        string json = File.ReadAllText(_libraryFile);

        _books = JsonSerializer.Deserialize<List<BookInfo>>(json)
                 ?? new List<BookInfo>();
    }

    public BookInfo ImportBook(string originalFile)
    {

        string hash = ComputeHash(originalFile);
        BookInfo? existingBook = _books.FirstOrDefault(book => book.FileHash == hash);

        if (existingBook != null)
        {
            existingBook.LastOpened = DateTime.Now;

            SaveLibrary();

            return existingBook;
        }

        Guid id = Guid.NewGuid();

        string folder = Path.Combine(_libraryFolder, id.ToString());

        Directory.CreateDirectory(folder);

        string destination = Path.Combine(folder, "book.docx");

        File.Copy(originalFile, destination);

        BookInfo book = new BookInfo
        {
            Id = id,
            Title = Path.GetFileNameWithoutExtension(originalFile),
            FilePath = destination,
            FileHash = hash,
            LastOpened = DateTime.Now,
            LastPage = 0,
            LastTextPosition = 0
        };

        _books.Add(book);
        SaveLibrary();

        return book;
    }

    private static string ComputeHash(string filePath)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);

        byte[] hash = sha.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }


}