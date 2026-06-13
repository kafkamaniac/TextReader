using System.Windows;
using Wpf = System.Windows.Documents;
using Docx = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

namespace TextReader.Services;

public static class DocxMapper
{
    public static Docx.Run ToDocxRun(Wpf.Run wpfRun)
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
}