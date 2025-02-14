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
        private readonly FrameNavigationService<SpecialtiesPageViewModel> _specialtiesNavigationService;

        public FrameNavigationViewModel(
            FrameNavigationStore frameNavigationStore,
            FrameNavigationService<HomePageViewModel> homeNavigationService,
            FrameNavigationService<FacultiesPageViewModel> facultiesNavigationService,
            FrameNavigationService<HoldingPageViewModel> holdingNavigationService,
            FrameNavigationService<WorkersPageViewModel> usersNavigationService,
            FrameNavigationService<SpecialtiesPageViewModel> specialtiesNavigationService
            )
        {
            _homeNavigationService = homeNavigationService;
            _facultiesNavigationService = facultiesNavigationService;
            _holdingNavigationService = holdingNavigationService;
            _usersNavigationService = usersNavigationService;
            _specialtiesNavigationService = specialtiesNavigationService;
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

        public async Task AdminNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Academicians":
                    break;
                case "Specialties":
                    await _specialtiesNavigationService.NavigateAsync();
                    break;
                case "Disciplines":
                    break;
                case "Groups":
                    break;
                default:
                    break;
            }
        }
    }
}
