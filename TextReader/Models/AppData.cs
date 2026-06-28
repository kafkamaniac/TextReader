using System.Collections.Generic;
using TextReader.Models;

namespace TextReader.Models
{
    public class AppData
    {
        public string Theme { get; set; } = "Light";

        public List<VocabularyItem> Vocabulary { get; set; }
            = new();

        public string VoiceName { get; set; } = "";
        public int SpeechRate { get; set; } = 0;

    }
}