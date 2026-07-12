using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using NovaSortPro.Models;

namespace NovaSortPro.Services
{
    public class UndoService
    {
        private readonly DatabaseService _dbService;

        public UndoService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public void RegisterMove(string sessionId, string originalPath, string newPath, long fileSize)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO undo_journal (session_id, operation_time, original_path, new_path, file_size, is_restored)
                VALUES ($session, $time, $orig, $new, $size, 0);";
            command.Parameters.AddWithValue("$session", sessionId);
            command.Parameters.AddWithValue("$time", DateTime.Now.ToString("O"));
            command.Parameters.AddWithValue("$orig", originalPath);
            command.Parameters.AddWithValue("$new", newPath);
            command.Parameters.AddWithValue("$size", fileSize);
            command.ExecuteNonQuery();
        }

        public List<UndoRecord> GetActiveJournalEntries()
        {
            var entries = new List<UndoRecord>();
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, session_id, operation_time, original_path, new_path, file_size, is_restored 
                FROM undo_journal 
                WHERE is_restored = 0 
                ORDER BY id DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                entries.Add(new UndoRecord
                {
                    Id = reader.GetInt32(0),
                    SessionId = reader.GetString(1),
                    OperationTime = DateTime.Parse(reader.GetString(2)),
                    OriginalPath = reader.GetString(3),
                    NewPath = reader.GetString(4),
                    FileSize = reader.GetInt64(5),
                    IsRestored = reader.GetInt32(6) == 1
                });
            }
            return entries;
        }

        public List<string> GetSessions()
        {
            var sessions = new List<string>();
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT session_id FROM undo_journal WHERE is_restored = 0 ORDER BY operation_time DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessions.Add(reader.GetString(0));
            }
            return sessions;
        }

        public bool UndoLastOperation()
        {
            var sessions = GetSessions();
            if (sessions.Count == 0) return false;
            return UndoSession(sessions[0]);
        }

        public bool UndoSession(string sessionId)
        {
            var records = new List<UndoRecord>();
            using var connection = _dbService.GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT id, original_path, new_path 
                    FROM undo_journal 
                    WHERE session_id = $session AND is_restored = 0;";
                command.Parameters.AddWithValue("$session", sessionId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(new UndoRecord
                    {
                        Id = reader.GetInt32(0),
                        OriginalPath = reader.GetString(1),
                        NewPath = reader.GetString(2)
                    });
                }
            }

            if (records.Count == 0) return false;

            bool overallSuccess = true;

            foreach (var record in records)
            {
                try
                {
                    if (File.Exists(record.NewPath))
                    {
                        var dir = Path.GetDirectoryName(record.OriginalPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        // Move file back
                        File.Move(record.NewPath, record.OriginalPath, overwrite: false);

                        // Mark as restored in database
                        using var updateCmd = connection.CreateCommand();
                        updateCmd.CommandText = "UPDATE undo_journal SET is_restored = 1 WHERE id = $id;";
                        updateCmd.Parameters.AddWithValue("$id", record.Id);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        overallSuccess = false;
                    }
                }
                catch
                {
                    overallSuccess = false;
                }
            }

            return overallSuccess;
        }

        public bool UndoPreviousDay()
        {
            var records = new List<UndoRecord>();
            using var connection = _dbService.GetConnection();
            using (var command = connection.CreateCommand())
            {
                // Select records from the previous day (or older than 24 hours)
                var threshold = DateTime.Now.AddDays(-1).ToString("O");
                command.CommandText = @"
                    SELECT id, original_path, new_path 
                    FROM undo_journal 
                    WHERE operation_time >= $threshold AND is_restored = 0;";
                command.Parameters.AddWithValue("$threshold", threshold);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    records.Add(new UndoRecord
                    {
                        Id = reader.GetInt32(0),
                        OriginalPath = reader.GetString(1),
                        NewPath = reader.GetString(2)
                    });
                }
            }

            if (records.Count == 0) return false;

            bool overallSuccess = true;

            foreach (var record in records)
            {
                try
                {
                    if (File.Exists(record.NewPath))
                    {
                        var dir = Path.GetDirectoryName(record.OriginalPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        File.Move(record.NewPath, record.OriginalPath, overwrite: false);

                        using var updateCmd = connection.CreateCommand();
                        updateCmd.CommandText = "UPDATE undo_journal SET is_restored = 1 WHERE id = $id;";
                        updateCmd.Parameters.AddWithValue("$id", record.Id);
                        updateCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        overallSuccess = false;
                    }
                }
                catch
                {
                    overallSuccess = false;
                }
            }

            return overallSuccess;
        }
    }
}
