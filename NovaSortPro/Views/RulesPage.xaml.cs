using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using NovaSortPro.ViewModels;
using NovaSortPro.Models;

namespace NovaSortPro.Views
{
    public sealed partial class RulesPage : Page
    {
        public RulesViewModel ViewModel { get; }

        public RulesPage()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(RulesViewModel)) as RulesViewModel
                        ?? throw new InvalidOperationException("RulesViewModel not registered.");

            this.DataContext = ViewModel;
        }

        private void OnAddRuleClick(object sender, RoutedEventArgs e)
        {
            ViewModel.AddRuleCommand.Execute(null);
        }

        private void OnDeleteRuleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is RuleItem rule)
            {
                ViewModel.DeleteRuleCommand.Execute(rule);
            }
        }

        private void OnSaveRulesClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveRulesCommand.Execute(null);
        }

        private async void OnExportRulesClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON File", new[] { ".json" });
            savePicker.SuggestedFileName = "NovaSortPro_Rules";

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                ViewModel.ExportRulesCommand.Execute(file.Path);
            }
        }

        private async void OnImportRulesClick(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(((App)Application.Current).MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                ViewModel.ImportRulesCommand.Execute(file.Path);
            }
        }
    }
}
