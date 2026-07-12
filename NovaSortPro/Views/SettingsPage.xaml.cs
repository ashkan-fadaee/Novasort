using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NovaSortPro.ViewModels;

namespace NovaSortPro.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel
                        ?? throw new InvalidOperationException("SettingsViewModel not registered.");

            this.DataContext = ViewModel;

            InitializeSelectionStates();
        }

        private void InitializeSelectionStates()
        {
            // Language combo initialization
            if (ViewModel.SelectedLanguage == "fa-IR")
            {
                LanguageCombo.SelectedIndex = 1;
            }
            else
            {
                LanguageCombo.SelectedIndex = 0;
            }

            // Theme combo initialization
            ThemeCombo.SelectedIndex = ViewModel.SelectedTheme switch
            {
                "Light" => 0,
                "Dark" => 1,
                _ => 2
            };
        }

        private void OnLanguageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageCombo == null || ViewModel == null) return;

            var tag = (LanguageCombo.SelectedItem as ComboBoxItem)?.Tag as string;
            if (!string.IsNullOrEmpty(tag))
            {
                ViewModel.SelectedLanguage = tag;
            }
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeCombo == null || ViewModel == null) return;

            var theme = ThemeCombo.SelectedIndex switch
            {
                0 => "Light",
                1 => "Dark",
                _ => "System"
            };
            ViewModel.SelectedTheme = theme;
        }
    }
}
