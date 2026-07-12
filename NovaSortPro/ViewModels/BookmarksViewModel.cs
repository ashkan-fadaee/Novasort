using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaSortPro.Models;
using NovaSortPro.Services;

namespace NovaSortPro.ViewModels
{
    public class BookmarksViewModel : ObservableObject
    {
        private readonly BookmarkService _bookmarkService;
        private readonly LoggingService _logger;

        private ObservableCollection<Bookmark> _favorites = new();
        private ObservableCollection<Bookmark> _recents = new();
        private ObservableCollection<Bookmark> _pinned = new();

        private string _newFolderPath = string.Empty;
        private string _newFolderName = string.Empty;

        public BookmarksViewModel(BookmarkService bookmarkService, LoggingService logger)
        {
            _bookmarkService = bookmarkService;
            _logger = logger;

            AddFavoriteCommand = new RelayCommand(ExecuteAddFavorite);
            DeleteBookmarkCommand = new RelayCommand<Bookmark>(ExecuteDeleteBookmark);

            LoadBookmarks();
        }

        public ObservableCollection<Bookmark> Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        public ObservableCollection<Bookmark> Recents
        {
            get => _recents;
            set => SetProperty(ref _recents, value);
        }

        public ObservableCollection<Bookmark> Pinned
        {
            get => _pinned;
            set => SetProperty(ref _pinned, value);
        }

        public string NewFolderPath
        {
            get => _newFolderPath;
            set => SetProperty(ref _newFolderPath, value);
        }

        public string NewFolderName
        {
            get => _newFolderName;
            set => SetProperty(ref _newFolderName, value);
        }

        public ICommand AddFavoriteCommand { get; }
        public ICommand DeleteBookmarkCommand { get; }

        public void LoadBookmarks()
        {
            Favorites = new ObservableCollection<Bookmark>(_bookmarkService.GetBookmarks("Favorite"));
            Recents = new ObservableCollection<Bookmark>(_bookmarkService.GetBookmarks("Recent"));
            Pinned = new ObservableCollection<Bookmark>(_bookmarkService.GetBookmarks("Pinned"));
        }

        private void ExecuteAddFavorite()
        {
            if (string.IsNullOrWhiteSpace(NewFolderPath)) return;

            var name = string.IsNullOrWhiteSpace(NewFolderName) 
                ? System.IO.Path.GetFileName(NewFolderPath) 
                : NewFolderName;
            if (string.IsNullOrEmpty(name)) name = NewFolderPath;

            var bookmark = new Bookmark
            {
                Path = NewFolderPath,
                Name = name,
                Type = "Favorite",
                DateAdded = DateTime.Now
            };

            _bookmarkService.AddBookmark(bookmark);
            Favorites.Insert(0, bookmark);

            NewFolderPath = string.Empty;
            NewFolderName = string.Empty;
            _logger.Log($"Folder added to Favorites: {bookmark.Path}");
        }

        private void ExecuteDeleteBookmark(Bookmark? bookmark)
        {
            if (bookmark == null) return;

            _bookmarkService.DeleteBookmark(bookmark.Id);
            
            if (bookmark.Type == "Favorite") Favorites.Remove(bookmark);
            else if (bookmark.Type == "Recent") Recents.Remove(bookmark);
            else if (bookmark.Type == "Pinned") Pinned.Remove(bookmark);

            _logger.Log($"Deleted bookmark {bookmark.Name} ({bookmark.Type})");
        }
    }
}
