using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

public class HighlightAdorner : Adorner
{
    private Rect _rect;
    private readonly Brush _brush;
    private readonly Pen _pen;

    public HighlightAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        _brush = new SolidColorBrush(Color.FromArgb(80, 255, 230, 0));
        _pen = new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 200, 0)), 1);
    }

    public void Update(Rect rect)
    {
        _rect = rect;
        InvalidateVisual();
    }

    public void Clear()
    {
        _rect = Rect.Empty;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_rect == Rect.Empty)
            return;

        dc.DrawRectangle(_brush, _pen, _rect);
    }
}