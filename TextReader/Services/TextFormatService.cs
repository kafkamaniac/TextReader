using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TextReader.Services;

public static class TextFormatService
{
    public static void ToggleBold(RichTextBox editor)
    {
        var selection = editor.Selection;

        if (selection.IsEmpty)
            return;

        var current = selection.GetPropertyValue(TextElement.FontWeightProperty);

        if (current != DependencyProperty.UnsetValue &&
            current.Equals(FontWeights.Bold))
        {
            selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
        }
        else
        {
            selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }

        editor.Focus();
    }

    public static void ToggleItalic(RichTextBox editor)
    {
        var selection = editor.Selection;

        if (selection.IsEmpty)
            return;

        var current = selection.GetPropertyValue(TextElement.FontStyleProperty);

        if (current != DependencyProperty.UnsetValue &&
            current.Equals(FontStyles.Italic))
        {
            selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
        }
        else
        {
            selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
        }

        editor.Focus();
    }

    public static void ToggleUnderline(RichTextBox editor)
    {
        var selection = editor.Selection;

        if (selection.IsEmpty)
            return;

        var current = selection.GetPropertyValue(Inline.TextDecorationsProperty);

        if (current != DependencyProperty.UnsetValue &&
            current == TextDecorations.Underline)
        {
            selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }
        else
        {
            selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
        }

        editor.Focus();
    }

    public static void ToggleStrike(RichTextBox editor)
    {
        var selection = editor.Selection;

        if (selection.IsEmpty)
            return;

        var current = selection.GetPropertyValue(Inline.TextDecorationsProperty);

        if (current != DependencyProperty.UnsetValue &&
            current == TextDecorations.Strikethrough)
        {
            selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
        }
        else
        {
            selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
        }

        editor.Focus();
    }


    public static void AlignLeft(RichTextBox editor)
    {
        ApplyAlignment(editor, TextAlignment.Left);
    }

    public static void AlignCenter(RichTextBox editor)
    {
        ApplyAlignment(editor, TextAlignment.Center);
    }

    public static void AlignRight(RichTextBox editor)
    {
        ApplyAlignment(editor, TextAlignment.Right);
    }

    public static void Justify(RichTextBox editor)
    {
        ApplyAlignment(editor, TextAlignment.Justify);
    }

    private static void ApplyAlignment(
        RichTextBox editor,
        TextAlignment alignment)
    {
        TextSelection selection = editor.Selection;

        Paragraph? startParagraph =
            selection.Start.Paragraph;

        Paragraph? endParagraph =
            selection.End.Paragraph;

        if (startParagraph == null || endParagraph == null)
            return;

        Paragraph? current = startParagraph;

        while (current != null)
        {
            current.TextAlignment = alignment;

            if (current == endParagraph)
                break;

            current =
                current.NextBlock as Paragraph;
        }

        editor.Focus();
    }
}