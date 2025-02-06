using Client.Services;
using Client.Stores.NavigationStores;

namespace Client.ViewModels.NavigationViewModel
{
    public class FrameNavigationViewModel
    {
        private readonly FrameNavigationService<HomePageViewModel> _homeNavigationService;
        private readonly FrameNavigationService<FacultiesPageViewModel> _facultiesNavigationService;
        //private readonly NavigationService<UsersView> _usersNavigationService;
        //private readonly NavigationService<FacultiesView> _facultiesNavigationService;
        //private readonly NavigationService<ConductView> _conductNavigationService;
        //private readonly NavigationService<DatabaseView> _databaseNavigationService;
        //private readonly NavigationService<LogView> _logNavigationService;

        public FrameNavigationViewModel(
            FrameNavigationStore frameNavigationStore,
            FrameNavigationService<HomePageViewModel> homeNavigationService,
            FrameNavigationService<FacultiesPageViewModel> facultiesNavigationService
            )
        {
            _homeNavigationService = homeNavigationService;
            _facultiesNavigationService = facultiesNavigationService;
        }

        public async Task SupAdminNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Users":
                    break;
                case "Faculties":
                    await _facultiesNavigationService.NavigateAsync();
                    break;
                case "Conduct":
                    break;
                case "Database":
                    break;
                case "Log":
                    break;
                default:
                    break;
            }
        }
    }
}
