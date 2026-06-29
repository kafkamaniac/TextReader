using System.Windows;
using System.Windows.Media;

namespace TextReader.ReadingEngine;

public class ParagraphModel
{
    public string Text { get; set; } = "";

    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }

    public string FontFamily { get; set; } = "Segoe UI";
    public double FontSize { get; set; } = 12;

    public Color? Foreground { get; set; }

    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
}