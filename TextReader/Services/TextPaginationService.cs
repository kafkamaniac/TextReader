using System.Windows.Controls;
using System.Windows.Documents;
using TextReader.Models;
public class TextPaginationService
{
    public readonly ITextMeasurer _measurer;

    public TextPaginationService(ITextMeasurer measurer)
    {
        _measurer = measurer;
    }

    public List<string> BuildPages(string text)
    {
        var pages = new List<string>();

        int i = 0;

        while (i < text.Length)
        {
            string page = "";

            while (i < text.Length && _measurer.CanFit(page + text[i]))
            {
                page += text[i];
                i++;
            }

            pages.Add(page);
        }

        return pages;
    }
}