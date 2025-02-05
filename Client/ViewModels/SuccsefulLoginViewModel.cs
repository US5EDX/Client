using Client.Services;
using Client.Stores;

namespace Client.ViewModels
{
    public partial class SuccsefulLoginViewModel
    {
        private readonly NavigationService<SupAdminViewModel> _supAdminNavigationService;
        private readonly NavigationService<AdminViewModel> _adminNavigationService;
        private readonly NavigationService<LecturerViewModel> _lecturerNavigationService;
        private readonly NavigationService<StudentViewModel> _studentNavigationService;
        private readonly UserStore _userStore;

        public SuccsefulLoginViewModel(
            NavigationService<SupAdminViewModel> supAdminNavigationService,
            NavigationService<AdminViewModel> adminNavigationService,
            NavigationService<LecturerViewModel> lecturerNavigationService,
            NavigationService<StudentViewModel> studentNavigationService,
            UserStore userStore)
        {
            _supAdminNavigationService = supAdminNavigationService;
            _adminNavigationService = adminNavigationService;
            _lecturerNavigationService = lecturerNavigationService;
            _studentNavigationService = studentNavigationService;
            _userStore = userStore;
        }

        public void NavigateBasedOnRole()
        {
            switch (_userStore.Role)
            {
                case 1:
                    _supAdminNavigationService.Navigate();
                    break;
                case 2:
                    _adminNavigationService.Navigate();
                    break;
                case 3:
                    _lecturerNavigationService.Navigate();
                    break;
                case 4:
                    _studentNavigationService.Navigate();
                    break;
                default:
                    break;
            }
        }
    }
}
