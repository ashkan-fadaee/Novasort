using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using NovaSortPro.ViewModels;
using NovaSortPro.Models;

namespace NovaSortPro.Views
{
    public sealed partial class OrganizerPage : Page
    {
        public OrganizerViewModel ViewModel { get; }

        public OrganizerPage()
        {
            this.InitializeComponent();
            
            // Resolve ViewModel via standard ServiceProvider (supplied by App.xaml.cs / App.Current)
            ViewModel = ((App)Application.Current).Services.GetService(typeof(OrganizerViewModel)) as OrganizerViewModel 
                        ?? throw new InvalidOperationException("OrganizerViewModel not registered.");
            
            this.DataContext = ViewModel;

            // Configure conflict callback to prompt dialog if necessary
            ViewModel.ConflictCallback = ResolveConflictAsync;
        }

        private async void OnBrowseFolderClick(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            // Associate the Window handle with the FolderPicker (Mandatory for WinUI 3 desktop apps!)
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.TargetFolderPath = folder.Path;
            }
        }

        private async void OnScanClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.ScanCommand.ExecuteAsync(null);
        }

        private async void OnOrganizeClick(object sender, RoutedEventArgs e)
        {
            // Ask for user confirmation before starting, as requested by general rules
            var confirmDialog = new ContentDialog
            {
                Title = "Confirm Operation",
                Content = "Are you sure you want to begin organizing the files? Your files will be categorized and moved into appropriate subfolders safely offline.",
                PrimaryButtonText = "Begin Sorting",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ProgressBorder.Visibility = Visibility.Visible;
                await ViewModel.ApplySortCommand.ExecuteAsync(null);
                ProgressBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPauseResumeClick(object sender, RoutedEventArgs e)
        {
            ViewModel.PauseResumeCommand.Execute(null);
            PauseResumeBtn.Content = ViewModel.IsPaused ? "Resume" : "Pause";
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelCommand.Execute(null);
            ProgressBorder.Visibility = Visibility.Collapsed;
        }

        private void OnConflictOptionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConflictOptionGroup == null) return;
            ViewModel.SelectedConflictOption = ConflictOptionGroup.SelectedIndex switch
            {
                0 => ConflictOption.Rename,
                1 => ConflictOption.Replace,
                2 => ConflictOption.Skip,
                3 => ConflictOption.AskEveryTime,
                _ => ConflictOption.Rename
            };
        }

        private async Task<ConflictOption> ResolveConflictAsync(FileItem item)
        {
            // Prompt custom ContentDialog to ask for resolution
            var conflictDialog = new ContentDialog
            {
                Title = "Conflict Detected",
                Content = $"File already exists in destination:\n{item.Name}\n\nWhat would you like to do?",
                PrimaryButtonText = "Rename",
                SecondaryButtonText = "Replace",
                CloseButtonText = "Skip",
                XamlRoot = this.XamlRoot
            };

            var result = await conflictDialog.ShowAsync();
            return result switch
            {
                ContentDialogResult.Primary => ConflictOption.Rename,
                ContentDialogResult.Secondary => ConflictOption.Replace,
                _ => ConflictOption.Skip
            };
        }
    }
}
