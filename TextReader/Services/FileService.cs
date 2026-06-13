using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
using Wpf = System.Windows.Documents;



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
    public static FlowDocument LoadDocxAsFlowDocument(string path)
    {
        FlowDocument document = new FlowDocument();

        using WordprocessingDocument doc =
            WordprocessingDocument.Open(path, false);

        var body = doc.MainDocumentPart?.Document?.Body;

        if (body == null)
            return document;

        foreach (var para in body.Elements<Docx.Paragraph>())
        {
            var wpfParagraph = new Wpf.Paragraph();

            foreach (var run in para.Elements<Docx.Run>())
            {
                wpfParagraph.Inlines.Add(DocxMapper.ToWpfRun(run));
            }

            document.Blocks.Add(wpfParagraph);
        }

        return document;
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
                        newParagraph.Append(DocxMapper.ToDocxRun(wpfRun));
                    }
                }

                body.Append(newParagraph);
            }
        }

        mainPart.Document = new Docx.Document(body);
        mainPart.Document.Save();
    }

}