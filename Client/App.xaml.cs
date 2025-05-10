using Client.API;
using Client.Handlers;
using Client.HostBuilders;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.NavigationStores;
using Client.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            _host = Host.CreateDefaultBuilder()
                .AddViewModels()
                .AddPageViewModels()
                .ConfigureServices((context, services) =>
            {
                services.AddSingleton<NavigationStore>();
                services.AddSingleton<FrameNavigationStore>();
                services.AddSingleton<UserStore>();
                services.AddSingleton<DisciplineMainInfoStore>();
                services.AddSingleton<GroupInfoStore>();

                services.AddSingleton<ApiService>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddSingleton<DisciplineReaderService>();
                services.AddSingleton<StudentsReaderService>();
                services.AddSingleton<StudentInfoStore>();
                services.AddSingleton<LecturerInfoStore>();

                services.AddSingleton(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    return config.GetSection("Endpoints");
                });

                services.AddHttpClient();
                services.AddHttpClient<Endpoints>().AddHttpMessageHandler<TokenRefreshHandler>();
                services.AddTransient<TokenRefreshHandler>();

                services.AddSingleton((services) => new MainWindow()
                {
                    DataContext = services.GetRequiredService<MainViewModel>(),
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });
            }).Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            PaletteHelper helper = new PaletteHelper();
            Theme theme = helper.GetTheme();
            theme.SetPrimaryColor(System.Windows.Media.Color.FromRgb(63, 81, 181));
            helper.SetTheme(theme);

            _host.Start();

            var navService = _host.Services.GetRequiredService<NavigationService<LoginViewModel>>();
            navService.Navigate();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }
    }
}
