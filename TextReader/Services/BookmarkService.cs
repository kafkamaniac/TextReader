using System.IO;
using System.Text.Json;
using TextReader.Models;

public class BookmarkService
{
    private readonly string _file;

    private List<Bookmark> _bookmarks = new();

    public IReadOnlyList<Bookmark> Bookmarks => _bookmarks;

    public BookmarkService()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TextReader");

        Directory.CreateDirectory(folder);

        _file = Path.Combine(folder, "bookmarks.json");

        Load();
    }

    public void Add(Bookmark bookmark)
    {
        _bookmarks.Add(bookmark);
        Save();
    }

    public void Remove(Guid id)
    {
        _bookmarks.RemoveAll(x => x.Id == id);
        Save();
    }

    public void Update(Bookmark bookmark)
    {
        int index = _bookmarks.FindIndex(x => x.Id == bookmark.Id);

        if (index == -1)
            return;

        _bookmarks[index] = bookmark;
        Save();
    }

    public List<Bookmark> GetBookmarks(Guid bookId)
    {
        return _bookmarks
            .Where(x => x.BookId == bookId)
            .OrderBy(x => x.Page)
            .ToList();
    }

    private void Save()
    {
        string json = JsonSerializer.Serialize(
            _bookmarks,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_file, json);
    }

    private void Load()
    {
        if (!File.Exists(_file))
        {
            _bookmarks = new List<Bookmark>();
            return;
        }

        string json = File.ReadAllText(_file);

        _bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(json)
                     ?? new List<Bookmark>();
    }

}