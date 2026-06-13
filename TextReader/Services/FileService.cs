using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using System.IO;
using System.Text;

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
        SaveFileDialog dialog = new();

        dialog.Filter = "Text Files (*.txt)|*.txt";

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

        using (WordprocessingDocument doc = WordprocessingDocument.Open(path, false))
        {
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null || mainPart.Document == null || mainPart.Document.Body == null)
            {
                throw new InvalidDataException("The .docx file is missing required document structure.");
            }

            Body? body = mainPart.Document.Body;

            if (body == null)
            {
                throw new InvalidDataException("The .docx file is missing required document body.");
            }

            foreach (Paragraph paragraph in body.Elements<Paragraph>())
            {
                sb.AppendLine(paragraph.InnerText);
            }
        }

        return sb.ToString();
    }


    public static void SaveDocx(string path, string text)
    {
        using WordprocessingDocument doc =
            WordprocessingDocument.Create(
                path,
                WordprocessingDocumentType.Document);

        MainDocumentPart mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());

        Body? body = mainPart.Document.Body;

        Paragraph paragraph = new Paragraph();
        Run run = new Run();
        run.Append(new Text(text));

        paragraph.Append(run);
        body?.Append(paragraph);

        mainPart.Document.Save();
    }

}