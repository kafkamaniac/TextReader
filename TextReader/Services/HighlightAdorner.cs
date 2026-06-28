using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public class HighlightAdorner : Adorner
{
    private List<Rect> _rects = new();
    private bool _hasValue;

    public HighlightAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        IsHitTestVisible = false;
    }

    public void UpdateRange(TextPointer start, TextPointer end)
    {
        _rects.Clear();

        if (start == null || end == null)
        {
            _hasValue = false;
            return;
        }

        var navigator = start;

        while (navigator != null && navigator.CompareTo(end) < 0)
        {
            Rect rect = navigator.GetCharacterRect(LogicalDirection.Forward);

            if (!rect.IsEmpty &&
                !double.IsNaN(rect.X) &&
                !double.IsInfinity(rect.X))
            {
                _rects.Add(rect);
            }

            navigator = navigator.GetNextInsertionPosition(LogicalDirection.Forward);
        }

        _hasValue = _rects.Count > 0;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (!_hasValue)
            return;

        var brush = new SolidColorBrush(Color.FromArgb(60, 0, 120, 255));

        foreach (var rect in _rects)
        {
            drawingContext.DrawRectangle(brush, null, rect);
        }
    }
}