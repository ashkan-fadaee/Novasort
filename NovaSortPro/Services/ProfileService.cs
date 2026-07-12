using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using NovaSortPro.Models;

namespace NovaSortPro.Services
{
    public class ProfileService
    {
        private readonly DatabaseService _dbService;

        public ProfileService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<Profile> GetProfiles()
        {
            var profiles = new List<Profile>();
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, description, is_active FROM profiles ORDER BY id ASC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                profiles.Add(new Profile
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = reader.GetInt32(3) == 1
                });
            }
            return profiles;
        }

        public void AddProfile(Profile profile)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO profiles (name, description, is_active)
                VALUES ($name, $desc, $active);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$name", profile.Name);
            command.Parameters.AddWithValue("$desc", profile.Description);
            command.Parameters.AddWithValue("$active", profile.IsActive ? 1 : 0);

            profile.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void SetActiveProfile(int profileId)
        {
            using var connection = _dbService.GetConnection();
            using var transaction = connection.BeginTransaction();

            using (var deactivateAll = connection.CreateCommand())
            {
                deactivateAll.Transaction = transaction;
                deactivateAll.CommandText = "UPDATE profiles SET is_active = 0;";
                deactivateAll.ExecuteNonQuery();
            }

            using (var activateOne = connection.CreateCommand())
            {
                activateOne.Transaction = transaction;
                activateOne.CommandText = "UPDATE profiles SET is_active = 1 WHERE id = $id;";
                activateOne.Parameters.AddWithValue("$id", profileId);
                activateOne.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public Profile? GetActiveProfile()
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, name, description, is_active FROM profiles WHERE is_active = 1 LIMIT 1;";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Profile
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = true
                };
            }
            return null;
        }
    }
}
