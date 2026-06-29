namespace TextReader.ReadingEngine;

public class PageModel
{
    public int Index { get; set; }

    public int StartIndex { get; set; }
    public int EndIndex { get; set; }

    public List<ParagraphModel> Paragraphs { get; set; } = new();
}