using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public class HighlightService
{
    private TextRange? _lastRange;

    private static readonly Brush HighlightBrush =
        new SolidColorBrush(Color.FromRgb(173, 216, 230));

    public void Clear()
    {
        if (_lastRange != null)
        {
            _lastRange.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            _lastRange = null;
        }
    }

    public void HighlightWord(FlowDocument document, string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return;

        Clear();

        TextPointer pointer = document.ContentStart;

        while (pointer != null)
        {
            if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                string text = pointer.GetTextInRun(LogicalDirection.Forward);

                int index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                {
                    TextPointer start = pointer.GetPositionAtOffset(index);
                    TextPointer end = start.GetPositionAtOffset(word.Length);

                    _lastRange = new TextRange(start, end);
                    _lastRange.ApplyPropertyValue(TextElement.BackgroundProperty, HighlightBrush);

                    return;
                }
            }

            pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
        }
    }
}