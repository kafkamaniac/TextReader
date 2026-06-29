using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace TextReader.ReadingEngine;

public static class PageRenderer
{
    public static FlowDocument Render(PageModel page)
    {
        var doc = new FlowDocument();

        foreach (var p in page.Paragraphs)
        {
            var paragraph = new Paragraph
            {
                TextAlignment = p.Alignment
            };

            var run = new Run(p.Text)
            {
                FontSize = p.FontSize,
                FontFamily = new FontFamily(p.FontFamily)
            };

            if (p.Bold)
                run.FontWeight = FontWeights.Bold;

            if (p.Italic)
                run.FontStyle = FontStyles.Italic;

            if (p.Underline)
                run.TextDecorations = TextDecorations.Underline;

            if (p.Foreground.HasValue)
                run.Foreground = new SolidColorBrush(p.Foreground.Value);

            paragraph.Inlines.Add(run);

            doc.Blocks.Add(paragraph);
        }

        return doc;
    }
}