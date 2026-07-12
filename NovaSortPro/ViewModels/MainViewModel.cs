using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly LocalizationService _loc;
        private readonly ProfileService _profileService;
        private readonly UndoService _undoService;
        private readonly LoggingService _logger;

        private string _statusText = string.Empty;
        private bool _isBusy = false;
        private string _selectedFolderPath = string.Empty;
        private Profile? _currentProfile;
        private List<Profile> _profilesList = new();

        public MainViewModel(
            LocalizationService loc, 
            ProfileService profileService, 
            UndoService undoService,
            LoggingService logger)
        {
            _loc = loc;
            _profileService = profileService;
            _undoService = undoService;
            _logger = logger;

            _loc.LanguageChanged += (s, e) => {
                OnPropertyChanged(nameof(AppName));
                OnPropertyChanged(nameof(LayoutDirection));
                OnPropertyChanged(nameof(FontFamily));
                StatusText = _loc.Get("Ready");
            };

            UndoCommand = new RelayCommand(ExecuteUndo);
            LoadProfiles();
            StatusText = _loc.Get("Ready");
        }

        // Localization helpers
        public string AppName => _loc.Get("AppName");
        public string HeaderText => _loc.Get("SmartOrganizer");
        public Microsoft.UI.Xaml.FlowDirection LayoutDirection => _loc.LayoutDirection;
        public string FontFamily => _loc.FontFamilyName;

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, _loc.LocalizeNumbers(value));
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set => SetProperty(ref _selectedFolderPath, value);
        }

        public Profile? CurrentProfile
        {
            get => _currentProfile;
            set
            {
                if (SetProperty(ref _currentProfile, value) && value != null)
                {
                    _profileService.SetActiveProfile(value.Id);
                    _logger.Log($"Switched to profile: {value.Name}");
                }
            }
        }

        public List<Profile> ProfilesList
        {
            get => _profilesList;
            set => SetProperty(ref _profilesList, value);
        }

        public ICommand UndoCommand { get; }

        public void LoadProfiles()
        {
            ProfilesList = _profileService.GetProfiles();
            CurrentProfile = _profileService.GetActiveProfile();
        }

        private void ExecuteUndo()
        {
            IsBusy = true;
            StatusText = "Undoing last operation...";
            _logger.Log("Requesting Undo from main interface");

            try
            {
                bool success = _undoService.UndoLastOperation();
                if (success)
                {
                    StatusText = _loc.Get("UndoSuccess");
                    _logger.Log("Undo completed successfully", "INFO");
                }
                else
                {
                    StatusText = _loc.Get("UndoError");
                    _logger.Log("Undo completed with errors or no active sessions found", "WARN");
                }
            }
            catch (Exception ex)
            {
                StatusText = _loc.Get("UndoError");
                _logger.Log($"Undo failure: {ex.Message}", "ERROR");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
