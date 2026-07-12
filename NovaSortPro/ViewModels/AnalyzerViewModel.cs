using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class AnalyzerViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly LocalizationService _loc;
        private readonly LoggingService _logger;

        private string _targetFolderPath = string.Empty;
        private bool _isAnalyzing = false;
        private string _statusMessage = string.Empty;
        private ScanResult _analysisResult = new();

        private ObservableCollection<FileItem> _duplicates = new();
        private ObservableCollection<FileItem> _emptyFiles = new();
        private ObservableCollection<string> _emptyFolders = new();
        private ObservableCollection<FileItem> _largeFiles = new();
        private ObservableCollection<FileItem> _oldFiles = new();
        private ObservableCollection<FileItem> _unknownExts = new();

        private CancellationTokenSource? _cts;

        public AnalyzerViewModel(FileService fileService, LocalizationService loc, LoggingService logger)
        {
            _fileService = fileService;
            _loc = loc;
            _logger = logger;

            AnalyzeCommand = new AsyncRelayCommand(ExecuteAnalysisAsync);
            CancelCommand = new RelayCommand(ExecuteCancel);
            ExportTxtCommand = new RelayCommand<string>(ExecuteExportTxt);
            ExportCsvCommand = new RelayCommand<string>(ExecuteExportCsv);
            ExportPdfCommand = new RelayCommand<string>(ExecuteExportPdf);
        }

        public string TargetFolderPath
        {
            get => _targetFolderPath;
            set => SetProperty(ref _targetFolderPath, value);
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => SetProperty(ref _isAnalyzing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ScanResult AnalysisResult
        {
            get => _analysisResult;
            set => SetProperty(ref _analysisResult, value);
        }

        public ObservableCollection<FileItem> Duplicates
        {
            get => _duplicates;
            set => SetProperty(ref _duplicates, value);
        }

        public ObservableCollection<FileItem> EmptyFiles
        {
            get => _emptyFiles;
            set => SetProperty(ref _emptyFiles, value);
        }

        public ObservableCollection<string> EmptyFolders
        {
            get => _emptyFolders;
            set => SetProperty(ref _emptyFolders, value);
        }

        public ObservableCollection<FileItem> LargeFiles
        {
            get => _largeFiles;
            set => SetProperty(ref _largeFiles, value);
        }

        public ObservableCollection<FileItem> OldFiles
        {
            get => _oldFiles;
            set => SetProperty(ref _oldFiles, value);
        }

        public ObservableCollection<FileItem> UnknownExts
        {
            get => _unknownExts;
            set => SetProperty(ref _unknownExts, value);
        }

        public IAsyncRelayCommand AnalyzeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportTxtCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ExportPdfCommand { get; }

        private async Task ExecuteAnalysisAsync()
        {
            if (string.IsNullOrWhiteSpace(TargetFolderPath) || !Directory.Exists(TargetFolderPath))
            {
                _logger.Log("Analysis canceled: invalid directory path", "WARN");
                return;
            }

            IsAnalyzing = true;
            _cts = new CancellationTokenSource();
            StatusMessage = "Starting analysis...";

            try
            {
                var progress = new Progress<string>(p => StatusMessage = p);
                var result = await _fileService.AnalyzeDirectoryAsync(TargetFolderPath, progress, _cts.Token);

                AnalysisResult = result;

                // Bind UI lists
                Duplicates = new ObservableCollection<FileItem>(result.DuplicateFiles);
                EmptyFiles = new ObservableCollection<FileItem>(result.EmptyFiles);
                EmptyFolders = new ObservableCollection<string>(result.EmptyFolders);
                LargeFiles = new ObservableCollection<FileItem>(result.LargeFiles);
                OldFiles = new ObservableCollection<FileItem>(result.OldFiles);
                UnknownExts = new ObservableCollection<FileItem>(result.UnknownExtensions);

                StatusMessage = "Analysis completed successfully!";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Analysis canceled by user.";
                _logger.Log("Analysis canceled by user", "INFO");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Analysis error: {ex.Message}";
                _logger.Log($"Analysis error: {ex.Message}", "ERROR");
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private void ExecuteCancel()
        {
            _cts?.Cancel();
            IsAnalyzing = false;
        }

        private void ExecuteExportTxt(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            _logger.ExportToTxt(path);
            _logger.Log($"Exported logs to TXT at {path}");
        }

        private void ExecuteExportCsv(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            _logger.ExportToCsv(path);
            _logger.Log($"Exported logs to CSV at {path}");
        }

        private void ExecuteExportPdf(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            _logger.ExportToPdf(path);
            _logger.Log($"Exported logs to PDF/HTML report at {path}");
        }
    }
}
