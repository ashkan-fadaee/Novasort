using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using NovaSortPro.ViewModels;
using NovaSortPro.Models;

namespace NovaSortPro.Views
{
    public sealed partial class BookmarksPage : Page
    {
        public BookmarksViewModel ViewModel { get; }

        public BookmarksPage()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(BookmarksViewModel)) as BookmarksViewModel
                        ?? throw new InvalidOperationException("BookmarksViewModel not registered.");

            this.DataContext = ViewModel;
        }

        private async void OnBrowseFolderClick(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.NewFolderPath = folder.Path;
            }
        }

        private void OnAddBookmarkClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AddFavoriteCommand.Execute(null);
        }

        private void OnDeleteBookmarkClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Bookmark bookmark)
            {
                ViewModel.DeleteBookmarkCommand.Execute(bookmark);
            }
        }
    }
}
