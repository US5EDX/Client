using Client.Services;
using Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Client.HostBuilders
{
    public static class AddViewModelsHostBuilderExtensions
    {
        public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(services =>
            {
                services.AddScoped<LoginViewModel>();
                services.AddSingleton<Func<LoginViewModel>>((s) => () => s.GetRequiredService<LoginViewModel>());
                services.AddSingleton<NavigationService<LoginViewModel>>();

                services.AddScoped<SupAdminViewModel>();
                services.AddSingleton<Func<SupAdminViewModel>>((s) => () => s.GetRequiredService<SupAdminViewModel>());
                services.AddSingleton<NavigationService<SupAdminViewModel>>();

                services.AddScoped<AdminViewModel>();
                services.AddSingleton<Func<AdminViewModel>>((s) => () => s.GetRequiredService<AdminViewModel>());
                services.AddSingleton<NavigationService<AdminViewModel>>();

                services.AddScoped<LecturerViewModel>();
                services.AddSingleton<Func<LecturerViewModel>>((s) => () => s.GetRequiredService<LecturerViewModel>());
                services.AddSingleton<NavigationService<LecturerViewModel>>();

                services.AddScoped<StudentViewModel>();
                services.AddSingleton<Func<StudentViewModel>>((s) => () => s.GetRequiredService<StudentViewModel>());
                services.AddSingleton<NavigationService<StudentViewModel>>();

                services.AddScoped<SuccsefulLoginViewModel>();
                services.AddSingleton<MainViewModel>();
            });

            return hostBuilder;
        }
    }
}
