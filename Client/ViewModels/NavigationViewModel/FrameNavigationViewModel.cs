using Client.Services;
using Client.Stores.NavigationStores;

namespace Client.ViewModels.NavigationViewModel
{
    public class FrameNavigationViewModel
    {
        private readonly FrameNavigationService<HomePageViewModel> _homeNavigationService;
        private readonly FrameNavigationService<FacultiesPageViewModel> _facultiesNavigationService;
        private readonly FrameNavigationService<HoldingPageViewModel> _holdingNavigationService;
        private readonly FrameNavigationService<WorkersPageViewModel> _usersNavigationService;

        public FrameNavigationViewModel(
            FrameNavigationStore frameNavigationStore,
            FrameNavigationService<HomePageViewModel> homeNavigationService,
            FrameNavigationService<FacultiesPageViewModel> facultiesNavigationService,
            FrameNavigationService<HoldingPageViewModel> holdingNavigationService,
            FrameNavigationService<WorkersPageViewModel> usersNavigationService
            )
        {
            _homeNavigationService = homeNavigationService;
            _facultiesNavigationService = facultiesNavigationService;
            _holdingNavigationService = holdingNavigationService;
            _usersNavigationService = usersNavigationService;
        }

        public async Task SupAdminNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Users":
                    await _usersNavigationService.NavigateAsync();
                    break;
                case "Faculties":
                    await _facultiesNavigationService.NavigateAsync();
                    break;
                case "Holding":
                    await _holdingNavigationService.NavigateAsync();
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
