using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class HistoryViewModel : ObservableObject
    {
        private readonly UndoService _undoService;
        private readonly LocalizationService _loc;
        private readonly LoggingService _logger;

        private ObservableCollection<UndoRecord> _journalEntries = new();
        private ObservableCollection<string> _sessions = new();
        private string? _selectedSession;

        public HistoryViewModel(UndoService undoService, LocalizationService loc, LoggingService logger)
        {
            _undoService = undoService;
            _loc = loc;
            _logger = logger;

            UndoLastCommand = new RelayCommand(ExecuteUndoLast);
            UndoSessionCommand = new RelayCommand(ExecuteUndoSession);
            UndoPreviousDayCommand = new RelayCommand(ExecuteUndoPreviousDay);

            LoadHistory();
        }

        public ObservableCollection<UndoRecord> JournalEntries
        {
            get => _journalEntries;
            set => SetProperty(ref _journalEntries, value);
        }

        public ObservableCollection<string> Sessions
        {
            get => _sessions;
            set => SetProperty(ref _sessions, value);
        }

        public string? SelectedSession
        {
            get => _selectedSession;
            set => SetProperty(ref _selectedSession, value);
        }

        public ICommand UndoLastCommand { get; }
        public ICommand UndoSessionCommand { get; }
        public ICommand UndoPreviousDayCommand { get; }

        public void LoadHistory()
        {
            JournalEntries = new ObservableCollection<UndoRecord>(_undoService.GetActiveJournalEntries());
            Sessions = new ObservableCollection<string>(_undoService.GetSessions());
        }

        private void ExecuteUndoLast()
        {
            _logger.Log("History Panel: Undoing last operation");
            var success = _undoService.UndoLastOperation();
            HandleUndoResult(success);
        }

        private void ExecuteUndoSession()
        {
            if (string.IsNullOrEmpty(SelectedSession)) return;
            _logger.Log($"History Panel: Undoing session {SelectedSession}");
            var success = _undoService.UndoSession(SelectedSession);
            HandleUndoResult(success);
        }

        private void ExecuteUndoPreviousDay()
        {
            _logger.Log("History Panel: Undoing previous day's operations");
            var success = _undoService.UndoPreviousDay();
            HandleUndoResult(success);
        }

        private void HandleUndoResult(bool success)
        {
            if (success)
            {
                _logger.Log("Undo operation executed successfully");
            }
            else
            {
                _logger.Log("Undo operation encountered some errors (some files might already be deleted or manual interference occurred)", "WARN");
            }
            LoadHistory(); // Refresh
        }
    }
}
