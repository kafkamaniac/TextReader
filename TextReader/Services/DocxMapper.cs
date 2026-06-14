using System.Windows;
using Wpf = System.Windows.Documents;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Windows.Media;

namespace TextReader.Services;

public static class DocxMapper
{
    //проходимся по куску текста с одинаковым форматированием
    public static Docx.Run ToDocxRun(Wpf.Run wpfRun)
    {
        var run = new Docx.Run();
        var text = new Docx.Text(wpfRun.Text ?? "")
        {
            Space = SpaceProcessingModeValues.Preserve
        };

        var props = BuildRunProperties(wpfRun);

        if (props != null && props.HasChildren)
            run.Append(props);

        run.Append(BuildText(wpfRun));

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

            if (runProps.FontSize != null)
            {
                if (double.TryParse(runProps.FontSize.Val, out double size))
                    wpfRun.FontSize = size / 2.0; 
            }

            if (runProps.RunFonts != null)
            {
                var font = runProps.RunFonts.Ascii?.Value;

                if (!string.IsNullOrEmpty(font))
                {
                    wpfRun.FontFamily = new FontFamily(font);
                }
            }
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

    #region RUN

    private static Docx.Text BuildText(Wpf.Run wpfRun)
    {
        return new Docx.Text(wpfRun.Text ?? "")
        {
            Space = SpaceProcessingModeValues.Preserve
        };
    }

    private static Docx.RunProperties? BuildRunProperties(Wpf.Run wpfRun)
    {
        var props = new Docx.RunProperties();

        ApplyFontWeight(wpfRun, props);
        ApplyFontStyle(wpfRun, props);
        ApplyTextDecorations(wpfRun, props);
        ApplyFontSize(wpfRun, props);
        ApplyFontFamily(wpfRun, props);

        return props;
    }

    private static void ApplyFontWeight(Wpf.Run wpfRun, Docx.RunProperties props)
    {
        if (wpfRun.FontWeight == FontWeights.Bold)
            props.Append(new Docx.Bold());
    }

    private static void ApplyFontStyle(Wpf.Run wpfRun, Docx.RunProperties props)
    {
        if (wpfRun.FontStyle == FontStyles.Italic)
            props.Append(new Docx.Italic());
    }

    private static void ApplyTextDecorations(Wpf.Run wpfRun, Docx.RunProperties props)
    {
        if (wpfRun.TextDecorations == TextDecorations.Underline)
            props.Append(new Docx.Underline());

        if (wpfRun.TextDecorations == TextDecorations.Strikethrough)
            props.Append(new Docx.Strike());
    }

    private static void ApplyFontSize(Wpf.Run wpfRun, Docx.RunProperties props)
    {
        double size = wpfRun.FontSize;

        if (size > 0)
        {
            int halfPoints = (int)(size * 2);
            props.Append(new Docx.FontSize() { Val = halfPoints.ToString() });
        }
    }


    #endregion

    #region FONTS STYLE
    //ШРИИИИФТы

    private static void ApplyFontFamily(Wpf.Run wpfRun, Docx.RunProperties props)
    {
        if (wpfRun.FontFamily != null)
        {
            props.Append(new Docx.RunFonts()
            {
                Ascii = wpfRun.FontFamily.Source,
                HighAnsi = wpfRun.FontFamily.Source
            });
        }
    }


    #endregion

}