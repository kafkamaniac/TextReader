using System.Windows;
using Wpf = System.Windows.Documents;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace TextReader.Services;

public static class DocxMapper
{
    public static Docx.Run ToDocxRun(Wpf.Run wpfRun) //проходимся по куску текста с одинаковым форматированием
                                                     
    {
        var run = new Docx.Run();

        var text = new Docx.Text(wpfRun.Text ?? "")
        {
            Space = SpaceProcessingModeValues.Preserve
        };

        var props = new Docx.RunProperties();

        if (wpfRun.FontWeight == FontWeights.Bold)
            props.Append(new Docx.Bold());

        if (wpfRun.FontStyle == FontStyles.Italic)
            props.Append(new Docx.Italic());

        if (wpfRun.TextDecorations == System.Windows.TextDecorations.Underline)
            props.Append(new Docx.Underline());

        if (wpfRun.TextDecorations == System.Windows.TextDecorations.Strikethrough)
            props.Append(new Docx.Strike());

        if (props.HasChildren)
            run.Append(props);

        run.Append(text);

        return run;
    }

    public static Wpf.Run ToWpfRun(Docx.Run run)
    {
        var wpfRun = new Wpf.Run();

        var textElement = run.GetFirstChild<Docx.Text>();
        if (textElement != null)
            wpfRun.Text = textElement.Text;

        var runProps = run.RunProperties;

        if (runProps != null)
        {
            if (runProps.Bold != null)
                wpfRun.FontWeight = FontWeights.Bold;

            if (runProps.Italic != null)
                wpfRun.FontStyle = FontStyles.Italic;

            if (runProps.Underline != null)
                wpfRun.TextDecorations = TextDecorations.Underline;

            if (runProps.Strike != null)
                wpfRun.TextDecorations = TextDecorations.Strikethrough;
        }

        return wpfRun;
    }

    public static Docx.Body ToDocxBody(Wpf.FlowDocument document)
    {
        var body = new Docx.Body();

        //using Wpf = System.Windows.Documents; то есть для каждого блока в нашем FlowDocument, мы проверяем,
        //является ли он параграфом. Если да, то мы конвертируем его в формат OpenXML и добавляем в Body нашего документа.
        foreach (Wpf.Block block in document.Blocks)
        {
            if (block is Wpf.Paragraph paragraph)
            {
                body.Append(ToDocxParagraph(paragraph)); 
            }
        }
        return body; 
    }

    public static Docx.Paragraph ToDocxParagraph(Wpf.Paragraph paragraph)
    {
        var docxParagraph = new Docx.Paragraph();
        var props = new Docx.ParagraphProperties();

        switch (paragraph.TextAlignment)
        {
            case TextAlignment.Left:
                props.Append(
                    new Docx.Justification()
                    {
                        Val = Docx.JustificationValues.Left
                    });
                break;

            case TextAlignment.Center:
                props.Append(
                    new Docx.Justification()
                    {
                        Val = Docx.JustificationValues.Center
                    });
                break;

            case TextAlignment.Right:
                props.Append(
                    new Docx.Justification()
                    {
                        Val = Docx.JustificationValues.Right
                    });
                break;

            case TextAlignment.Justify:
                props.Append(
                    new Docx.Justification()
                    {
                        Val = Docx.JustificationValues.Both
                    });
                break;
        }

        docxParagraph.Append(props);

        foreach (Wpf.Inline inline in paragraph.Inlines)
        {
            if (inline is Wpf.Run run)
            {
                docxParagraph.Append(ToDocxRun(run));
            }
        }
        return docxParagraph;
    }
}