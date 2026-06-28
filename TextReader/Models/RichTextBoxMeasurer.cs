using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TextReader.Models
{
    public interface ITextMeasurer
    {
        bool CanFit(string text);
    }

    public class RichTextBoxMeasurer : ITextMeasurer
    {
        private readonly RichTextBox _box;

        public RichTextBoxMeasurer(RichTextBox box)
        {
            _box = box;
        }

        public bool CanFit(string text)
        {
            _box.Document.Blocks.Clear();
            _box.Document.Blocks.Add(new Paragraph(new Run(text)));
            _box.UpdateLayout();

            return _box.ExtentHeight <= _box.ViewportHeight;
        }
    }
}
