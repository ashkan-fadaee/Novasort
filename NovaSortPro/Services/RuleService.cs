using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using NovaSortPro.Models;

namespace NovaSortPro.Services
{
    public class RuleService
    {
        private readonly DatabaseService _dbService;

        public RuleService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public List<RuleItem> GetRules()
        {
            var rules = new List<RuleItem>();
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT id, pattern, target_folder, is_active, is_custom FROM rules ORDER BY pattern ASC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                rules.Add(new RuleItem
                {
                    Id = reader.GetInt32(0),
                    Pattern = reader.GetString(1),
                    TargetFolder = reader.GetString(2),
                    IsActive = reader.GetInt32(3) == 1,
                    IsCustom = reader.GetInt32(4) == 1
                });
            }
            return rules;
        }

        public void AddRule(RuleItem rule)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO rules (pattern, target_folder, is_active, is_custom)
                VALUES ($pattern, $target, $active, $custom);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$pattern", rule.Pattern.ToLowerInvariant());
            command.Parameters.AddWithValue("$target", rule.TargetFolder);
            command.Parameters.AddWithValue("$active", rule.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$custom", rule.IsCustom ? 1 : 0);

            rule.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void UpdateRule(RuleItem rule)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE rules
                SET pattern = $pattern, target_folder = $target, is_active = $active
                WHERE id = $id;";
            command.Parameters.AddWithValue("$pattern", rule.Pattern.ToLowerInvariant());
            command.Parameters.AddWithValue("$target", rule.TargetFolder);
            command.Parameters.AddWithValue("$active", rule.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("$id", rule.Id);
            command.ExecuteNonQuery();
        }

        public void DeleteRule(int id)
        {
            using var connection = _dbService.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM rules WHERE id = $id;";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        public string ExportRules()
        {
            var rules = GetRules();
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(rules, options);
        }

        public void ImportRules(string json)
        {
            var rules = JsonSerializer.Deserialize<List<RuleItem>>(json);
            if (rules == null) return;

            using var connection = _dbService.GetConnection();
            using var transaction = connection.BeginTransaction();
            
            // Delete existing rules first, or update/insert
            using (var clearCmd = connection.CreateCommand())
            {
                clearCmd.Transaction = transaction;
                clearCmd.CommandText = "DELETE FROM rules;";
                clearCmd.ExecuteNonQuery();
            }

            foreach (var rule in rules)
            {
                using var insertCmd = connection.CreateCommand();
                insertCmd.Transaction = transaction;
                insertCmd.CommandText = @"
                    INSERT INTO rules (pattern, target_folder, is_active, is_custom)
                    VALUES ($pattern, $target, $active, $custom);";
                insertCmd.Parameters.AddWithValue("$pattern", rule.Pattern.ToLowerInvariant());
                insertCmd.Parameters.AddWithValue("$target", rule.TargetFolder);
                insertCmd.Parameters.AddWithValue("$active", rule.IsActive ? 1 : 0);
                insertCmd.Parameters.AddWithValue("$custom", rule.IsCustom ? 1 : 0);
                insertCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
