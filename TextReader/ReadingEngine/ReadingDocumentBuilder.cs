using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace TextReader.ReadingEngine;

public static class ReadingDocumentBuilder
{
    public static ReadingDocument Build(FlowDocument document)
    {
        var result = new ReadingDocument();

        foreach (var block in document.Blocks)
        {
            if (block is not Paragraph paragraph)
                continue;

            AddParagraph(result, paragraph);
        }

        return result;
    }

    private static void AddParagraph(ReadingDocument result, Paragraph paragraph)
    {
        result.Paragraphs.Add(ConvertParagraph(paragraph));
    }

    private static ParagraphModel ConvertParagraph(Paragraph paragraph)
    {
        var text = new System.Text.StringBuilder();

        foreach (var inline in paragraph.Inlines)
        {
            if (inline is Run run)
            {
                text.Append(run.Text);
            }
        }

        var firstRun = paragraph.Inlines.OfType<Run>().FirstOrDefault();

        return new ParagraphModel
        {
            Text = text.ToString(),

            Bold = firstRun?.FontWeight == FontWeights.Bold,
            Italic = firstRun?.FontStyle == FontStyles.Italic,
            Underline = firstRun?.TextDecorations == TextDecorations.Underline,

            FontSize = firstRun?.FontSize ?? 12,
            FontFamily = firstRun?.FontFamily?.Source ?? "Segoe UI",

            Foreground = (firstRun?.Foreground as SolidColorBrush)?.Color,

            Alignment = paragraph.TextAlignment
        };
    }

}