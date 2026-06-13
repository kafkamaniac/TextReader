using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Wpf = System.Windows.Documents;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
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
        SaveFileDialog dialog = new()
        {
            Filter = "Text Files (*.txt)|*.txt"
        };

        if (dialog.ShowDialog() != true)
            return null;

        File.WriteAllText(dialog.FileName, text);
        return dialog.FileName;
    }

    public static string LoadDocx(string path)
    {
        if (!File.Exists(path))
            return string.Empty;

        FileInfo info = new(path);

        if (info.Length == 0)
            return string.Empty;

        StringBuilder sb = new();

        using (WordprocessingDocument doc =
               WordprocessingDocument.Open(path, false))
        {
            var body = doc.MainDocumentPart?.Document?.Body;

            if (body == null)
                return string.Empty;

            foreach (Docx.Paragraph paragraph in body.Elements<Docx.Paragraph>())
            {
                sb.AppendLine(paragraph.InnerText);
            }
        }

        return sb.ToString();
    }

    public static void SaveDocx(string path, string text)
    {
        using WordprocessingDocument doc =
            WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Docx.Document(new Docx.Body());

        var body = mainPart.Document.Body;

        var paragraph = new Docx.Paragraph();
        var run = new Docx.Run();

        run.Append(new Docx.Text(text));

        paragraph.Append(run);
        body.Append(paragraph);

        mainPart.Document.Save();
    }

    public static void SaveDocxFromRichTextBox(string path, RichTextBox editor)
    {
        using WordprocessingDocument doc =
            WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);

        var mainPart = doc.AddMainDocumentPart();

        var body = new Docx.Body();

        Wpf.FlowDocument flow = editor.Document;

        foreach (Wpf.Block block in flow.Blocks)
        {
            if (block is Wpf.Paragraph paragraph)
            {
                var newParagraph = new Docx.Paragraph();

                foreach (Wpf.Inline inline in paragraph.Inlines)
                {
                    if (inline is Wpf.Run wpfRun)
                    {
                        var run = new Docx.Run();

                        var text = new Docx.Text(wpfRun.Text ?? "")
                        {
                            Space = SpaceProcessingModeValues.Preserve
                        };

                        var props = new Docx.RunProperties();

                        // BOLD
                        if (wpfRun.FontWeight == FontWeights.Bold)
                            props.Append(new Docx.Bold());

                        // ITALIC
                        if (wpfRun.FontStyle == FontStyles.Italic)
                            props.Append(new Docx.Italic());

                        // UNDERLINE
                        if (wpfRun.TextDecorations == System.Windows.TextDecorations.Underline)
                            props.Append(new Docx.Underline());

                        // STRIKE
                        if (wpfRun.TextDecorations == System.Windows.TextDecorations.Strikethrough)
                            props.Append(new Docx.Strike());

                        if (props.HasChildren)
                            run.Append(props);

                        run.Append(text);
                        newParagraph.Append(run);
                    }
                }

                body.Append(newParagraph);
            }
        }

        mainPart.Document = new Docx.Document(body);
        mainPart.Document.Save();
    }
}