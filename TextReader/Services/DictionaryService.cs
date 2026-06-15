using Microsoft.Data.Sqlite;

public class DictionaryService
{
    private readonly SqliteConnection _conn;

    public DictionaryService()
    {
        _conn = new SqliteConnection("Data Source=dict.db");
        _conn.Open();
    }

    public string Translate(string word)
    {
        var cmd = _conn.CreateCommand();
        cmd.CommandText =
            "SELECT translation FROM dictionary WHERE word = $word";

        cmd.Parameters.AddWithValue("$word", word.ToLower());

        return cmd.ExecuteScalar()?.ToString() ?? "Не найдено";
    }
}