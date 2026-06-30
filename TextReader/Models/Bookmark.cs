namespace TextReader.Models;

public class Bookmark
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BookId { get; set; }

    public string Name { get; set; } = "";

    public int Page { get; set; }

    public long TextPosition { get; set; }

    public DateTime Created { get; set; } = DateTime.Now;
}