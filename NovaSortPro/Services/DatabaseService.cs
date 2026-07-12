using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace NovaSortPro.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;

        public DatabaseService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(appData, "NovaSortPro");
            Directory.CreateDirectory(appDir);
            _dbPath = Path.Combine(appDir, "novasort_pro.db");
            InitializeDatabase();
        }

        public string DbPath => _dbPath;

        public SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            return conn;
        }

        private void InitializeDatabase()
        {
            using var connection = GetConnection();
            
            // Enable WAL mode for concurrency and performance
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA journal_mode=WAL;";
                command.ExecuteNonQuery();
            }

            // Create Rules Table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS rules (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        pattern TEXT NOT NULL UNIQUE,
                        target_folder TEXT NOT NULL,
                        is_active INTEGER NOT NULL DEFAULT 1,
                        is_custom INTEGER NOT NULL DEFAULT 1
                    );";
                command.ExecuteNonQuery();
            }

            // Create Bookmarks Table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS bookmarks (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        path TEXT NOT NULL UNIQUE,
                        name TEXT NOT NULL,
                        type TEXT NOT NULL,
                        date_added TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();
            }

            // Create Profiles Table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS profiles (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE,
                        description TEXT,
                        is_active INTEGER NOT NULL DEFAULT 0
                    );";
                command.ExecuteNonQuery();
            }

            // Create Undo Journal Table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS undo_journal (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        session_id TEXT NOT NULL,
                        operation_time TEXT NOT NULL,
                        original_path TEXT NOT NULL,
                        new_path TEXT NOT NULL,
                        file_size INTEGER NOT NULL,
                        is_restored INTEGER NOT NULL DEFAULT 0
                    );";
                command.ExecuteNonQuery();
            }

            // Create Settings Table
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS settings (
                        key TEXT PRIMARY KEY,
                        value TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();
            }

            SeedDefaultRules(connection);
            SeedDefaultProfiles(connection);
        }

        private void SeedDefaultRules(SqliteConnection connection)
        {
            // Check if we already have rules, if not seed them
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM rules;";
            var count = Convert.ToInt64(checkCmd.ExecuteScalar());
            if (count > 0) return;

            var defaultRules = new (string Pattern, string Target)[]
            {
                // Images
                ("jpg", "Images"), ("jpeg", "Images"), ("png", "Images"), ("gif", "Images"),
                ("bmp", "Images"), ("webp", "Images"), ("svg", "Images"), ("heic", "Images"),
                // Videos
                ("mp4", "Videos"), ("mkv", "Videos"), ("avi", "Videos"), ("mov", "Videos"),
                ("wmv", "Videos"), ("webm", "Videos"),
                // Music
                ("mp3", "Music"), ("wav", "Music"), ("flac", "Music"), ("ogg", "Music"), ("aac", "Music"),
                // Documents
                ("pdf", "Documents"), ("doc", "Documents"), ("docx", "Documents"), ("xls", "Documents"),
                ("xlsx", "Documents"), ("ppt", "Documents"), ("pptx", "Documents"), ("txt", "Documents"),
                ("csv", "Documents"),
                // Archives
                ("zip", "Archives"), ("rar", "Archives"), ("7z", "Archives"), ("tar", "Archives"), ("gz", "Archives"),
                // Programs
                ("exe", "Programs"), ("msi", "Programs"),
                // Android
                ("apk", "Android"), ("aab", "Android"),
                // Books
                ("epub", "Books"), ("mobi", "Books"),
                // Source Code
                ("py", "Source Code"), ("cpp", "Source Code"), ("c", "Source Code"), ("cs", "Source Code"),
                ("java", "Source Code"), ("kt", "Source Code"), ("js", "Source Code"), ("ts", "Source Code"),
                ("html", "Source Code"), ("css", "Source Code"), ("php", "Source Code"), ("go", "Source Code"),
                ("rs", "Source Code"),
                // Design
                ("psd", "Design"), ("ai", "Design"), ("fig", "Design"), ("xd", "Design"),
                // Fonts
                ("ttf", "Fonts"), ("otf", "Fonts"),
                // 3D
                ("blend", "3D"), ("fbx", "3D"), ("obj", "3D"), ("stl", "3D"),
                // Database
                ("db", "Database"), ("sqlite", "Database"), ("sql", "Database"),
                // ISO
                ("iso", "ISO"),
                // Backups
                ("bak", "Backups")
            };

            using var transaction = connection.BeginTransaction();
            foreach (var rule in defaultRules)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    INSERT INTO rules (pattern, target_folder, is_active, is_custom)
                    VALUES ($pattern, $target, 1, 0);";
                insertCmd.Parameters.AddWithValue("$pattern", rule.Pattern);
                insertCmd.Parameters.AddWithValue("$target", rule.Target);
                insertCmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        private void SeedDefaultProfiles(SqliteConnection connection)
        {
            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM profiles;";
            var count = Convert.ToInt64(checkCmd.ExecuteScalar());
            if (count > 0) return;

            var defaultProfiles = new (string Name, string Desc, bool IsActive)[]
            {
                ("Downloads", "Standard Download folder organization", true),
                ("Gaming", "Organize game files and ROMs", false),
                ("School", "Academic documents, PDF, books, and essays", false),
                ("Work", "Work spreadsheets, source code, and reports", false),
                ("Media", "Organize music, pictures, and video libraries", false),
                ("Portable Drive", "Heavy-duty files and backups organization", false)
            };

            using var transaction = connection.BeginTransaction();
            foreach (var profile in defaultProfiles)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    INSERT INTO profiles (name, description, is_active)
                    VALUES ($name, $desc, $active);";
                insertCmd.Parameters.AddWithValue("$name", profile.Name);
                insertCmd.Parameters.AddWithValue("$desc", profile.Desc);
                insertCmd.Parameters.AddWithValue("$active", profile.IsActive ? 1 : 0);
                insertCmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
    }
}
