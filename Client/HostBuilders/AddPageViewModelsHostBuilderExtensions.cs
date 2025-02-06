using Client.Services;
using Client.ViewModels;
using Client.ViewModels.NavigationViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Client.HostBuilders
{
    public static class AddPageViewModelsHostBuilderExtensions
    {
        public static IHostBuilder AddPageViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<HomePageViewModel>();
                services.AddSingleton<Func<HomePageViewModel>>((s) => () => s.GetRequiredService<HomePageViewModel>());
                services.AddSingleton<FrameNavigationService<HomePageViewModel>>();

                services.AddTransient<FacultiesPageViewModel>();
                services.AddSingleton<Func<FacultiesPageViewModel>>((s) => () => s.GetRequiredService<FacultiesPageViewModel>());
                services.AddSingleton<FrameNavigationService<FacultiesPageViewModel>>();

                services.AddScoped<FrameNavigationViewModel>();
            });

            return hostBuilder;
        }
    }
}
