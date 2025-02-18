using Client.API;
using Client.Handlers;
using Client.HostBuilders;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.NavigationStores;
using Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
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
            _host = Host.CreateDefaultBuilder()
                .AddViewModels()
                .AddPageViewModels()
                .ConfigureServices((context, services) =>
            {
                services.AddSingleton<NavigationStore>();
                services.AddSingleton<FrameNavigationStore>();
                services.AddSingleton<UserStore>();

                services.AddSingleton<ApiService>();
                services.AddSingleton<IMessageService, MessageService>();
                services.AddSingleton<DisciplineReaderService>();

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
