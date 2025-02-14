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

                services.AddTransient<HoldingPageViewModel>();
                services.AddSingleton<Func<HoldingPageViewModel>>((s) => () => s.GetRequiredService<HoldingPageViewModel>());
                services.AddSingleton<FrameNavigationService<HoldingPageViewModel>>();

                services.AddTransient<WorkersPageViewModel>();
                services.AddSingleton<Func<WorkersPageViewModel>>((s) => () => s.GetRequiredService<WorkersPageViewModel>());
                services.AddSingleton<FrameNavigationService<WorkersPageViewModel>>();

                services.AddTransient<SpecialtiesPageViewModel>();
                services.AddSingleton<Func<SpecialtiesPageViewModel>>((s) => () => s.GetRequiredService<SpecialtiesPageViewModel>());
                services.AddSingleton<FrameNavigationService<SpecialtiesPageViewModel>>();

                services.AddTransient<AcademiciansPageViewModel>();
                services.AddSingleton<Func<AcademiciansPageViewModel>>((s) => () => s.GetRequiredService<AcademiciansPageViewModel>());
                services.AddSingleton<FrameNavigationService<AcademiciansPageViewModel>>();

                services.AddScoped<FrameNavigationViewModel>();
            });

            return hostBuilder;
        }
    }
}
