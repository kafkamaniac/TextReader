using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

    public class SpeechHighlightService
    {
        private readonly RichTextBox _box;
        private HighlightAdorner _adorner;
        private AdornerLayer _layer;

        public SpeechHighlightService(RichTextBox box)
        {
            _box = box;
        }

    public void Init(RichTextBox box)
    {
        _layer = AdornerLayer.GetAdornerLayer(box);

        if (_layer == null)
        {
            MessageBox.Show("AdornerLayer = NULL");
            return;
        }

        _adorner = new HighlightAdorner(box);
        _layer.Add(_adorner);
    }

    public void Highlight(TextPointer pointer)
        {
            if (_adorner == null || pointer == null)
                return;

        _box.Dispatcher.BeginInvoke(new Action(() =>
        {
            _box.UpdateLayout();

                Rect rect = pointer.GetCharacterRect(LogicalDirection.Forward);

                if (rect.IsEmpty)
                    return;

                rect = new Rect(0, rect.Top, _box.ActualWidth, rect.Height);

                _adorner.Update(rect);
            }), DispatcherPriority.Background);
    }

        public void Clear()
        {
            _adorner?.Clear();
        }

    public void Dispose()
    {
        if (_layer != null && _adorner != null)
        {
            _layer.Remove(_adorner);
        }

        _adorner = null;
        _layer = null;
    }
}
