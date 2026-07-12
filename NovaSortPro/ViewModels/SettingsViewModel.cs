using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly LocalizationService _loc;
        private readonly DatabaseService _db;
        private readonly LoggingService _logger;

        private string _selectedLanguage = "en-US";
        private string _selectedTheme = "System";

        public SettingsViewModel(LocalizationService loc, DatabaseService db, LoggingService logger)
        {
            _loc = loc;
            _db = db;
            _logger = logger;

            _selectedLanguage = _loc.CurrentLanguage;
            LoadSettings();
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    _loc.CurrentLanguage = value;
                    SaveSetting("Language", value);
                    _logger.Log($"Language setting changed to: {value}");
                }
            }
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    SaveSetting("Theme", value);
                    _logger.Log($"Theme setting changed to: {value}");
                    // Theme switching in WinUI 3 can be bound or set directly on RootElement
                }
            }
        }

        public string DatabasePath => _db.DbPath;
        public string LogFilePath => _logger.GetLogFilePath();

        // Localized strings
        public string LanguageLabel => _loc.Get("Language");
        public string ThemeLabel => _loc.Get("Theme");
        public string OfflineLabel => _loc.Get("OfflineDisclaimer");

        private void LoadSettings()
        {
            using var connection = _db.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT key, value FROM settings;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var key = reader.GetString(0);
                var val = reader.GetString(1);

                if (key == "Language")
                {
                    SelectedLanguage = val;
                }
                else if (key == "Theme")
                {
                    SelectedTheme = val;
                }
            }
        }

        private void SaveSetting(string key, string value)
        {
            using var connection = _db.GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO settings (key, value)
                VALUES ($key, $val);";
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$val", value);
            command.ExecuteNonQuery();
        }
    }
}
