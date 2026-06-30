using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextReader.Models
{
    public class BookInfo
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string FilePath { get; set; }

        public string FileHash { get; set; } = "";

        public DateTime LastOpened { get; set; }

        public int LastPage { get; set; }

        public long LastTextPosition { get; set; }
    }

}
