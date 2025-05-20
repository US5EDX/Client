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

                services.AddTransient<GroupsPageViewModel>();
                services.AddSingleton<Func<GroupsPageViewModel>>((s) => () => s.GetRequiredService<GroupsPageViewModel>());
                services.AddSingleton<FrameNavigationService<GroupsPageViewModel>>();

                services.AddTransient<GroupPageViewModel>();
                services.AddSingleton<Func<GroupPageViewModel>>((s) => () => s.GetRequiredService<GroupPageViewModel>());
                services.AddSingleton<FrameNavigationService<GroupPageViewModel>>();

                services.AddTransient<DisciplinesPageViewModel>();
                services.AddSingleton<Func<DisciplinesPageViewModel>>((s) => () => s.GetRequiredService<DisciplinesPageViewModel>());
                services.AddSingleton<FrameNavigationService<DisciplinesPageViewModel>>();

                services.AddTransient<AllStudentChoicesViewModel>();
                services.AddSingleton<Func<AllStudentChoicesViewModel>>((s) => () => s.GetRequiredService<AllStudentChoicesViewModel>());
                services.AddSingleton<FrameNavigationService<AllStudentChoicesViewModel>>();

                services.AddTransient<StudentYearChoicesViewModel>();
                services.AddSingleton<Func<StudentYearChoicesViewModel>>((s) => () => s.GetRequiredService<StudentYearChoicesViewModel>());
                services.AddSingleton<FrameNavigationService<StudentYearChoicesViewModel>>();

                services.AddTransient<StudentChoosingPageViewModel>();
                services.AddSingleton<Func<StudentChoosingPageViewModel>>((s) => () => s.GetRequiredService<StudentChoosingPageViewModel>());
                services.AddSingleton<FrameNavigationService<StudentChoosingPageViewModel>>();

                services.AddTransient<DisciplinesForStudentViewModel>();
                services.AddSingleton<Func<DisciplinesForStudentViewModel>>((s) => () => s.GetRequiredService<DisciplinesForStudentViewModel>());
                services.AddSingleton<FrameNavigationService<DisciplinesForStudentViewModel>>();

                services.AddTransient<StudentChoicesViewModel>();
                services.AddSingleton<Func<StudentChoicesViewModel>>((s) => () => s.GetRequiredService<StudentChoicesViewModel>());
                services.AddSingleton<FrameNavigationService<StudentChoicesViewModel>>();

                services.AddTransient<SettingsPageViewModel>();
                services.AddSingleton<Func<SettingsPageViewModel>>((s) => () => s.GetRequiredService<SettingsPageViewModel>());
                services.AddSingleton<FrameNavigationService<SettingsPageViewModel>>();

                services.AddScoped<FrameNavigationViewModel>();
            });

            return hostBuilder;
        }
    }
}
