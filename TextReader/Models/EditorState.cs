using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextReader.Models
{
    public class EditorState
    {
        public string? CurrentFilePath { get; set; }

        public bool IsModified { get; set; }

        public bool IsLoading { get; set; }
    }
}
