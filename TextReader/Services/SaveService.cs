using System.IO;
using System.Text.Json;
using TextReader.Models;

namespace TextReader.Services
{
    public static class SaveService
    {
        private static readonly string SavePath =
            "userdata.json";

        public static void Save(AppData data)
        {
            string json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(SavePath, json);
        }

        public static AppData Load()
        {
            if (!File.Exists(SavePath))
                return new AppData();

            string json = File.ReadAllText(SavePath);

            return JsonSerializer.Deserialize<AppData>(json)
                   ?? new AppData();
        }
    }
}