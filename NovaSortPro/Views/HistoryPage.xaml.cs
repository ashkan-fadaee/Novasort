using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NovaSortPro.ViewModels;

namespace NovaSortPro.Views
{
    public sealed partial class HistoryPage : Page
    {
        public HistoryViewModel ViewModel { get; }

        public HistoryPage()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(HistoryViewModel)) as HistoryViewModel
                        ?? throw new InvalidOperationException("HistoryViewModel not registered.");

            this.DataContext = ViewModel;
        }

        private async void OnUndoLastClick(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Revert Last Sorting?",
                Content = "Are you sure you want to undo the last file sorting session? Files will be moved back to their original locations.",
                PrimaryButtonText = "Restore Files",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.UndoLastCommand.Execute(null);
            }
        }

        private async void OnUndoPreviousDayClick(object sender, RoutedEventArgs e)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Revert Previous 24h?",
                Content = "Are you sure you want to undo all file sorting operations that took place during the last 24 hours?",
                PrimaryButtonText = "Restore Files",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.UndoPreviousDayCommand.Execute(null);
            }
        }

        private async void OnUndoSessionClick(object sender, RoutedEventArgs e)
        {
            if (SessionComboBox.SelectedItem == null) return;

            var confirmDialog = new ContentDialog
            {
                Title = "Revert Selected Session?",
                Content = $"Are you sure you want to undo the operations belonging to sorting session: {SessionComboBox.SelectedItem}?",
                PrimaryButtonText = "Restore Files",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.UndoSessionCommand.Execute(null);
            }
        }
    }
}
