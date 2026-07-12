using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class OrganizerViewModel : ObservableObject
    {
        private readonly FileService _fileService;
        private readonly LocalizationService _loc;
        private readonly BookmarkService _bookmarkService;
        private readonly LoggingService _logger;

        private string _targetFolderPath = string.Empty;
        private bool _isScanning = false;
        private bool _isSorting = false;
        private bool _isPaused = false;
        private int _progressValue = 0;

        private List<FileItem> _scannedFilesRaw = new();
        private ObservableCollection<FileItem> _filesPreview = new();

        // Sort parameters
        private string _sortField = "Name"; // Name, Size, Extension, Date
        private bool _sortAscending = true;

        // Filter parameters
        private string _searchQuery = string.Empty;
        private string _filterCategory = "All";
        private ObservableCollection<string> _categories = new() { "All" };

        private CancellationTokenSource? _cts;

        public OrganizerViewModel(
            FileService fileService, 
            LocalizationService loc, 
            BookmarkService bookmarkService,
            LoggingService logger)
        {
            _fileService = fileService;
            _loc = loc;
            _bookmarkService = bookmarkService;
            _logger = logger;

            ScanCommand = new AsyncRelayCommand(ExecuteScanAsync);
            ApplySortCommand = new AsyncRelayCommand(ExecuteSortAndMoveAsync);
            PauseResumeCommand = new RelayCommand(ExecutePauseResume);
            CancelCommand = new RelayCommand(ExecuteCancel);

            _loc.LanguageChanged += (s, e) => {
                OnPropertyChanged(nameof(TotalFilesLabel));
                OnPropertyChanged(nameof(TotalSizeLabel));
                OnPropertyChanged(nameof(EstTimeLabel));
            };
        }

        public string TargetFolderPath
        {
            get => _targetFolderPath;
            set => SetProperty(ref _targetFolderPath, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public bool IsSorting
        {
            get => _isSorting;
            set => SetProperty(ref _isSorting, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public ObservableCollection<FileItem> FilesPreview
        {
            get => _filesPreview;
            set => SetProperty(ref _filesPreview, value);
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public string SortField
        {
            get => _sortField;
            set
            {
                if (SetProperty(ref _sortField, value))
                    ApplySortingAndFiltering();
            }
        }

        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (SetProperty(ref _sortAscending, value))
                    ApplySortingAndFiltering();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    ApplySortingAndFiltering();
            }
        }

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                if (SetProperty(ref _filterCategory, value))
                    ApplySortingAndFiltering();
            }
        }

        // Stats Labels
        public string TotalFilesLabel => $"{_loc.Get("TotalFiles")}: {_loc.LocalizeNumbers(_scannedFilesRaw.Count.ToString())}";
        public string TotalSizeLabel => $"{_loc.Get("TotalSize")}: {FileItem.FormatSize(_scannedFilesRaw.Sum(f => f.Size))}";
        public string EstTimeLabel => $"{_loc.Get("EstimatedTime")}: {_loc.LocalizeNumbers(CalculateEstimatedTime())}";

        public IAsyncRelayCommand ScanCommand { get; }
        public IAsyncRelayCommand ApplySortCommand { get; }
        public ICommand PauseResumeCommand { get; }
        public ICommand CancelCommand { get; }

        public ConflictOption SelectedConflictOption { get; set; } = ConflictOption.Rename;
        public Func<FileItem, Task<ConflictOption>>? ConflictCallback { get; set; }

        private async Task ExecuteScanAsync()
        {
            if (string.IsNullOrWhiteSpace(TargetFolderPath) || !Directory.Exists(TargetFolderPath))
            {
                _logger.Log("Scan skipped: Directory invalid or not specified", "WARN");
                return;
            }

            IsScanning = true;
            _cts = new CancellationTokenSource();
            _scannedFilesRaw.Clear();
            FilesPreview.Clear();

            _bookmarkService.AddRecent(TargetFolderPath);

            try
            {
                var files = await _fileService.ScanDirectoryAsync(
                    TargetFolderPath, 
                    new Progress<int>(p => ProgressValue = p), 
                    _cts.Token);

                _scannedFilesRaw = files;
                
                // Extract unique categories for filter combobox
                var uniqueCats = files.Select(f => f.Category).Distinct().OrderBy(c => c).ToList();
                Categories.Clear();
                Categories.Add("All");
                foreach (var cat in uniqueCats)
                {
                    Categories.Add(cat);
                }

                ApplySortingAndFiltering();
                UpdateStats();
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Scan operation was cancelled by user", "INFO");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error scanning directory: {ex.Message}", "ERROR");
            }
            finally
            {
                IsScanning = false;
                ProgressValue = 100;
            }
        }

        private async Task ExecuteSortAndMoveAsync()
        {
            if (_scannedFilesRaw.Count == 0) return;

            IsSorting = true;
            IsPaused = false;
            ProgressValue = 0;
            _cts = new CancellationTokenSource();

            _logger.Log($"Beginning sorting in folder: {TargetFolderPath}", "INFO");

            try
            {
                // Conflict resolution callback fallback if not supplied by UI
                var resolveConflict = ConflictCallback ?? (async item => {
                    return await Task.FromResult(SelectedConflictOption);
                });

                await _fileService.ProcessSortingAsync(
                    _scannedFilesRaw,
                    SelectedConflictOption,
                    resolveConflict,
                    new Progress<int>(p => ProgressValue = p),
                    _cts.Token
                );

                _logger.Log("Successfully organized all files", "INFO");
                // Refresh scan to show current remaining files
                await ExecuteScanAsync();
            }
            catch (OperationCanceledException)
            {
                _logger.Log("Sorting operation cancelled", "INFO");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error executing sorting: {ex.Message}", "ERROR");
            }
            finally
            {
                IsSorting = false;
                IsPaused = false;
                ProgressValue = 100;
            }
        }

        private void ExecutePauseResume()
        {
            if (IsPaused)
            {
                _fileService.Resume();
                IsPaused = false;
            }
            else
            {
                _fileService.Pause();
                IsPaused = true;
            }
        }

        private void ExecuteCancel()
        {
            _cts?.Cancel();
            _fileService.Resume(); // Break pause loop if locked
            IsPaused = false;
            IsSorting = false;
            IsScanning = false;
            _logger.Log("Pending operation aborted by user", "WARN");
        }

        private void UpdateStats()
        {
            OnPropertyChanged(nameof(TotalFilesLabel));
            OnPropertyChanged(nameof(TotalSizeLabel));
            OnPropertyChanged(nameof(EstTimeLabel));
        }

        private string CalculateEstimatedTime()
        {
            // Estimate ~30ms per file move.
            double totalSeconds = _scannedFilesRaw.Count * 0.03;
            if (totalSeconds < 1) return "1 second";
            if (totalSeconds < 60) return $"{(int)totalSeconds} seconds";
            var minutes = (int)(totalSeconds / 60);
            var seconds = (int)(totalSeconds % 60);
            return $"{minutes}m {seconds}s";
        }

        private void ApplySortingAndFiltering()
        {
            IEnumerable<FileItem> query = _scannedFilesRaw;

            // Search query filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(f => f.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                         f.Extension.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            // Category filter
            if (FilterCategory != "All")
            {
                query = query.Where(f => f.Category.Equals(FilterCategory, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting
            query = SortField switch
            {
                "Name" => SortAscending ? query.OrderBy(f => f.Name) : query.OrderByDescending(f => f.Name),
                "Size" => SortAscending ? query.OrderBy(f => f.Size) : query.OrderByDescending(f => f.Size),
                "Extension" => SortAscending ? query.OrderBy(f => f.Extension) : query.OrderByDescending(f => f.Extension),
                "Date" => SortAscending ? query.OrderBy(f => f.DateModified) : query.OrderByDescending(f => f.DateModified),
                _ => query
            };

            FilesPreview.Clear();
            foreach (var item in query)
            {
                FilesPreview.Add(item);
            }
        }
    }
}
