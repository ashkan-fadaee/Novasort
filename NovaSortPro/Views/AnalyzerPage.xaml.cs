using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using NovaSortPro.ViewModels;

namespace NovaSortPro.Views
{
    public sealed partial class AnalyzerPage : Page
    {
        public AnalyzerViewModel ViewModel { get; }

        public AnalyzerPage()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(AnalyzerViewModel)) as AnalyzerViewModel
                        ?? throw new InvalidOperationException("AnalyzerViewModel not registered.");

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
                ViewModel.TargetFolderPath = folder.Path;
            }
        }

        private async void OnAnalyzeClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.AnalyzeCommand.ExecuteAsync(null);
        }

        private async void OnExportTxtClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new[] { ".txt" });
            savePicker.SuggestedFileName = "NovaSortPro_Log";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                ViewModel.ExportTxtCommand.Execute(file.Path);
            }
        }

        private async void OnExportCsvClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV Table", new[] { ".csv" });
            savePicker.SuggestedFileName = "NovaSortPro_Log";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                ViewModel.ExportCsvCommand.Execute(file.Path);
            }
        }

        private async void OnExportPdfClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("HTML Report (PDF Ready)", new[] { ".html" });
            savePicker.SuggestedFileName = "NovaSortPro_Report";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                ViewModel.ExportPdfCommand.Execute(file.Path);
            }
        }
    }
}
