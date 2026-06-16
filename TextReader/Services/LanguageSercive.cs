using System;
using System.Windows;

namespace TextReader.Services
{
    public static class LanguageService
    {
        public static void ChangeLanguage(string language)
        {
            string path = language switch
            {
                "English" => "Languages/English.xaml",
                _ => "Languages/Russian.xaml"
            };

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var oldLanguage = dictionaries
                .FirstOrDefault(d =>
                    d.Source != null &&
                    d.Source.OriginalString.Contains("Languages"));

            if (oldLanguage != null)
                dictionaries.Remove(oldLanguage);

            dictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(path, UriKind.Relative)
            });
        }
    }
}