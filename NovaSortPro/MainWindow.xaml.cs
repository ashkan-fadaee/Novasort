using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NovaSortPro.ViewModels;
using NovaSortPro.Views;

namespace NovaSortPro
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = ((App)Application.Current).Services.GetService(typeof(MainViewModel)) as MainViewModel
                        ?? throw new InvalidOperationException("MainViewModel not registered.");

            this.DataContext = ViewModel;

            // Default navigation
            ContentFrame.Navigate(typeof(OrganizerPage));
        }

        private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
                MainNavView.Header = "Settings";
            }
            else if (args.InvokedItemContainer != null)
            {
                var tag = args.InvokedItemContainer.Tag as string;
                switch (tag)
                {
                    case "Organizer":
                        ContentFrame.Navigate(typeof(OrganizerPage));
                        MainNavView.Header = "Smart Organizer";
                        break;
                    case "Analyzer":
                        ContentFrame.Navigate(typeof(AnalyzerPage));
                        MainNavView.Header = "System Analyzer";
                        break;
                    case "Rules":
                        ContentFrame.Navigate(typeof(RulesPage));
                        MainNavView.Header = "Smart Rules";
                        break;
                    case "Bookmarks":
                        ContentFrame.Navigate(typeof(BookmarksPage));
                        MainNavView.Header = "Bookmarks & Favorites";
                        break;
                    case "History":
                        ContentFrame.Navigate(typeof(HistoryPage));
                        MainNavView.Header = "History & Reversion";
                        break;
                }
            }
        }
    }
}
