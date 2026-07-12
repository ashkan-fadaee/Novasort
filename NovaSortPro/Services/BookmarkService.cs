using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using NovaSortPro.Models;

namespace NovaSortPro.Services
{
    public class BookmarkService
    {
        private readonly DatabaseService _dbService;

        public BookmarkService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<Bookmark> GetBookmarks(string? type = null)
        {
            var bookmarks = new List<Bookmark>();
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            
            if (string.IsNullOrEmpty(type))
            {
                command.CommandText = "SELECT id, path, name, type, date_added FROM bookmarks ORDER BY date_added DESC;";
            }
            else
            {
                command.CommandText = "SELECT id, path, name, type, date_added FROM bookmarks WHERE type = $type ORDER BY date_added DESC;";
                command.Parameters.AddWithValue("$type", type);
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                bookmarks.Add(new Bookmark
                {
                    Id = reader.GetInt32(0),
                    Path = reader.GetString(1),
                    Name = reader.GetString(2),
                    Type = reader.GetString(3),
                    DateAdded = DateTime.Parse(reader.GetString(4))
                });
            }
            return bookmarks;
        }

        public void AddBookmark(Bookmark bookmark)
        {
            try
            {
                using var connection = _dbService.GetConnection();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO bookmarks (path, name, type, date_added)
                    VALUES ($path, $name, $type, $date);
                    SELECT last_insert_rowid();";
                command.Parameters.AddWithValue("$path", bookmark.Path);
                command.Parameters.AddWithValue("$name", bookmark.Name);
                command.Parameters.AddWithValue("$type", bookmark.Type);
                command.Parameters.AddWithValue("$date", bookmark.DateAdded.ToString("O"));

                bookmark.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            catch
            {
                // Ignore key conflicts for simplicity
            }
        }

        public void DeleteBookmark(int id)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM bookmarks WHERE id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        public void AddRecent(string path)
        {
            var name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(name)) name = path;

            AddBookmark(new Bookmark
            {
                Path = path,
                Name = name,
                Type = "Recent",
                DateAdded = DateTime.Now
            });

            // Enforce size limit on recents (e.g. keep top 10)
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM bookmarks 
                WHERE type = 'Recent' AND id NOT IN (
                    SELECT id FROM bookmarks 
                    WHERE type = 'Recent' 
                    ORDER BY date_added DESC 
                    LIMIT 10
                );";
            command.ExecuteNonQuery();
        }
    }
}
