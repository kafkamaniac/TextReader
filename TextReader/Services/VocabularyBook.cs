using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextReader.Models;

namespace TextReader.Services
{
    public static class VocabularyBook
    {
        public static ObservableCollection<VocabularyItem> Items { get; } = new();

        public static void Add(string word, string translation)
        {
            if (Items.Any(x => x.Word == word))
                return;

            Items.Add(new VocabularyItem
            {
                Word = word,
                Translation = translation
            });
        }
    }
}
