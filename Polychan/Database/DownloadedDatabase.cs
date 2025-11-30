using Microsoft.Data.Sqlite;

namespace Polychan.App.Database;

public class DownloadedDatabase
{
    private readonly string m_connectionString;

    public DownloadedDatabase(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        m_connectionString = $"Data Source={dbPath}";
    }
    
    public void Initialize()
    {
        using var conn = new SqliteConnection(m_connectionString);
        conn.Open();

        var cmd1 = conn.CreateCommand();
        cmd1.CommandText =
            """
            CREATE TABLE IF NOT EXISTS downloaded_threads (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                thread_id INTEGER NOT NULL,
                board TEXT NOT NULL,
                downloaded_at TEXT NOT NULL,
                folder_path TEXT NOT NULL,
                UNIQUE(thread_id, board)
            );
            """;

        cmd1.ExecuteNonQuery();
        
        var cmd2 = conn.CreateCommand();
        cmd2.CommandText =
            """
            CREATE TABLE IF NOT EXISTS downloaded_posts (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              thread_id INTEGER NOT NULL,
              board TEXT NOT NULL,
              post_no INTEGER NOT NULL,
              has_attachment INTEGER NOT NULL,
              attachment_path TEXT,
              json_path TEXT NOT NULL,
              FOREIGN KEY(thread_id, board) REFERENCES downloaded_threads(thread_id, board)  
            );
            """;
        
        cmd2.ExecuteNonQuery();
    }
    
    public void SaveVisit(FChan.Models.PostId threadId, string board, string json, byte[] thumbnail)
    {
        using var conn = new SqliteConnection(m_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText =
            """
            INSERT INTO thread_history (thread_id, board, json, thumbnail, visited_at)
            VALUES ($thread_id, $board, $json, $thumbnail, $ts)
            ON CONFLICT(thread_id, board) DO UPDATE SET
                json = $json,
                visited_at = $ts;
            """;
        cmd.Parameters.AddWithValue("$thread_id", threadId.Value);
        cmd.Parameters.AddWithValue("$board", board);
        cmd.Parameters.AddWithValue("$json", json);
        cmd.Parameters.AddWithValue("$thumbnail", thumbnail);
        cmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));

        cmd.ExecuteNonQuery();
    }
}