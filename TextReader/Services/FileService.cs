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
        //создаём пустой контейнер для документа

        using WordprocessingDocument doc =
            WordprocessingDocument.Open(path, false);
        //открываем существующий документ по указанному пути в режиме только для чтения,
        //используя класс WordprocessingDocument из OpenXML SDK

        var body = doc.MainDocumentPart?.Document?.Body;

        if (body == null)
            return document;
       

        foreach (var para in body.Elements<Docx.Paragraph>())
        {
            var wpfParagraph = new Wpf.Paragraph();
            // Получаем свойства абзаца, чтобы определить выравнивание текста
            var paraProps = para.ParagraphProperties;

            var alignment = paraProps?.Justification?.Val?.Value;

            if (alignment == Docx.JustificationValues.Center)
            {
                wpfParagraph.TextAlignment = TextAlignment.Center;
            }
            else if (alignment == Docx.JustificationValues.Right)
            {
                wpfParagraph.TextAlignment = TextAlignment.Right;
            }
            else if (alignment == Docx.JustificationValues.Both)
            {
                wpfParagraph.TextAlignment = TextAlignment.Justify;
            }
            else
            {
                wpfParagraph.TextAlignment = TextAlignment.Left;
            }

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
            WordprocessingDocument.Create(
                path,
                WordprocessingDocumentType.Document); 
        //создаём док файл по указанному пути, указывая его тип (в данном случае - документ).
        //внутри он пока пустой, но мы будем добавлять в него содержимое из редактора

        var mainPart = doc.AddMainDocumentPart();
        //в переменную mainPart сохраняем результат вызова метода AddMainDocumentPart,
        //который добавляет в документ основную часть (main document part) и возвращает её
        //то есть у документа есть части (например, основная часть, части для стилей, для изображений и т.д.),
        //и мы добавляем основную часть, которая будет содержать текст нашего документа
        //то есть document.xml
        //это и есть MainDocumentPart

        mainPart.Document = 
            new Docx.Document(
                DocxMapper.ToDocxBody(editor.Document));
        //через ToDocxBody мы преобразуем FlowDocument из редактора в Body для OpenXML,
        //а затем создаём новый объект Document,
        //передавая ему этот Body, и присваиваем его свойству Document основной части

        mainPart.Document.Save();
        //сохраняем изменения в основной части документа, что фактически сохраняет весь документ
    }

}