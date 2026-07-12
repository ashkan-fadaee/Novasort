using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using NovaSortPro.Services;
using NovaSortPro.ViewModels;

namespace NovaSortPro
{
    public partial class App : Application
    {
        private Window? m_window;
        public IServiceProvider Services { get; private set; } = null!;

        public App()
        {
            this.InitializeComponent();

            // Set up Dependency Injection
            var serviceCollection = new ServiceCollection();

            // Services
            serviceCollection.AddSingleton<DatabaseService>();
            serviceCollection.AddSingleton<RuleService>();
            serviceCollection.AddSingleton<BookmarkService>();
            serviceCollection.AddSingleton<ProfileService>();
            serviceCollection.AddSingleton<UndoService>();
            serviceCollection.AddSingleton<LoggingService>();
            serviceCollection.AddSingleton<LocalizationService>();
            serviceCollection.AddSingleton<FileService>();

            // ViewModels
            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddTransient<OrganizerViewModel>();
            serviceCollection.AddTransient<AnalyzerViewModel>();
            serviceCollection.AddTransient<RulesViewModel>();
            serviceCollection.AddTransient<BookmarksViewModel>();
            serviceCollection.AddTransient<HistoryViewModel>();
            serviceCollection.AddTransient<SettingsViewModel>();

            Services = serviceCollection.BuildServiceProvider();

            // Configure Static Locator fallback if needed by XAML bindings
            ViewModelLocator.Initialize(Services);
        }

        public Window MainWindow => m_window ?? throw new InvalidOperationException("App has not launched yet.");

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }

    /// <summary>
    /// Helper locator to resolve ViewModels in environments where pure XAML requires bindings
    /// </summary>
    public class ViewModelLocator
    {
        private static IServiceProvider? _provider;

        public static void Initialize(IServiceProvider provider)
        {
            _provider = provider;
        }

        public MainViewModel Main => Resolve<MainViewModel>();
        public OrganizerViewModel Organizer => Resolve<OrganizerViewModel>();
        public AnalyzerViewModel Analyzer => Resolve<AnalyzerViewModel>();
        public RulesViewModel Rules => Resolve<RulesViewModel>();
        public BookmarksViewModel Bookmarks => Resolve<BookmarksViewModel>();
        public HistoryViewModel History => Resolve<HistoryViewModel>();
        public SettingsViewModel Settings => Resolve<SettingsViewModel>();

        private static T Resolve<T>() where T : class
        {
            if (_provider == null) throw new InvalidOperationException("Locator not initialized.");
            return _provider.GetRequiredService<T>();
        }
    }
}
