using System.Windows.Documents;

public static class FlowDocumentNavigator
{
    public static TextPointer GetPointerAtOffset(FlowDocument doc, int offset)
    {
        TextPointer pointer = doc.ContentStart;
        int current = 0;

        while (pointer != null)
        {
            if (pointer.GetPointerContext(LogicalDirection.Forward)
                == TextPointerContext.Text)
            {
                string text = pointer.GetTextInRun(LogicalDirection.Forward);

                if (current + text.Length >= offset)
                {
                    return pointer.GetPositionAtOffset(offset - current);
                }

                current += text.Length;
                pointer = pointer.GetPositionAtOffset(text.Length);
            }
            else
            {
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
        }

        return doc.ContentEnd;
    }

    public static TextPointer FindWordPointer(FlowDocument doc, string word)
    {
        TextPointer pointer = doc.ContentStart;

        while (pointer != null)
        {
            if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                string text = pointer.GetTextInRun(LogicalDirection.Forward);

                int index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);

                if (index >= 0)
                    return pointer.GetPositionAtOffset(index);
            }

            pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
        }

        return null;
    }

}