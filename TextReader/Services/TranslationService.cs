using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace TextReader.Services
{
    public class TranslationService
    {
        private static readonly HttpClient client = new();

        public async Task<string> TranslateText(string text)
        {
            var req = new
            {
                q = text,
                source = "en",
                target = "ru",
                format = "text"
            };

            string json = JsonSerializer.Serialize(req);


            var resp = await client.PostAsync(
                "https://libretranslate.com/translate",
                new StringContent(json, Encoding.UTF8, "application/json"));

            string result = await resp.Content.ReadAsStringAsync();
            MessageBox.Show(result);

            using var doc = JsonDocument.Parse(result);

            return doc.RootElement.GetProperty("translatedText").GetString();
        }
    }
}