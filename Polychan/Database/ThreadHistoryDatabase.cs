using Microsoft.Data.Sqlite;

namespace Polychan.App.Database;

public class ThreadHistoryEntry
{
    public long Id { get; set; }
    public int ThreadId { get; set; }
    public string Board { get; set; }
    public string Json { get; set; }
    public byte[]? Thumbnail { get; set; }
    public DateTime VisitedAt { get; set; }
}

public class ThreadHistoryDatabase
{
    private readonly string m_connectionString;

    public ThreadHistoryDatabase(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        m_connectionString = $"Data Source={dbPath}";
    }

    public void Initialize()
    {
        using var conn = new SqliteConnection(m_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE IF NOT EXISTS thread_history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                thread_id INTEGER NOT NULL,
                board TEXT NOT NULL,
                json TEXT NOT NULL,
                thumbnail BLOB,
                visited_at TEXT NOT NULL,
                UNIQUE(thread_id, board)
            ); 
            """;

        cmd.ExecuteNonQuery();
    }

    public void SaveVisit(long threadId, string board, string json, byte[] thumbnail)
    {
        using var conn = new SqliteConnection(m_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO thread_history (thread_id, board, json, thumbnail, visited_at)
            VALUES ($id, $board, $json, $thumbnail, $ts)
            ON CONFLICT(thread_id, board) DO UPDATE SET
                json = $json,
                visited_at = $ts;
            """;
        cmd.Parameters.AddWithValue("$id", threadId);
        cmd.Parameters.AddWithValue("$board", board);
        cmd.Parameters.AddWithValue("$json", json);
        cmd.Parameters.AddWithValue("$thumbnail", thumbnail);
        cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));

        cmd.ExecuteNonQuery();
    }

    public List<ThreadHistoryEntry> LoadHistory()
    {
        var results = new List<ThreadHistoryEntry>();
        using var conn = new SqliteConnection(m_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            SELECT *
            FROM thread_history
            ORDER BY visited_at DESC;
            """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new ThreadHistoryEntry
            {
                Id = reader.GetInt64(0),
                ThreadId = reader.GetInt32(1),
                Board = reader.GetString(2),
                Json = reader.GetString(3),
                Thumbnail = reader.IsDBNull(4) ? null : reader.GetFieldValue<byte[]>(4),
                VisitedAt = DateTime.Parse(reader.GetString(5))
            });
        }

        return results;
    }
}