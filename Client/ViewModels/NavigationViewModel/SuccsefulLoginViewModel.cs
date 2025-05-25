using Client.Services;
using Client.Storages;
using Client.Stores;

namespace Client.ViewModels.NavigationViewModel
{
    public partial class SuccsefulLoginViewModel
    {
        private readonly NavigationService<LoginViewModel> _loginNavigationService;
        private readonly NavigationService<SupAdminViewModel> _supAdminNavigationService;
        private readonly NavigationService<AdminViewModel> _adminNavigationService;
        private readonly NavigationService<LecturerViewModel> _lecturerNavigationService;
        private readonly NavigationService<StudentViewModel> _studentNavigationService;
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;

        public SuccsefulLoginViewModel(
            NavigationService<LoginViewModel> loginNavigationService,
            NavigationService<SupAdminViewModel> supAdminNavigationService,
            NavigationService<AdminViewModel> adminNavigationService,
            NavigationService<LecturerViewModel> lecturerNavigationService,
            NavigationService<StudentViewModel> studentNavigationService,
            UserStore userStore,
            ApiService apiService)
        {
            _loginNavigationService = loginNavigationService;
            _supAdminNavigationService = supAdminNavigationService;
            _adminNavigationService = adminNavigationService;
            _lecturerNavigationService = lecturerNavigationService;
            _studentNavigationService = studentNavigationService;
            _userStore = userStore;
            _apiService = apiService;
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

        public async Task<string?> Logout()
        {
            var delResult = TokenStorage.DeleteToken();

            if (!delResult)
                return "Сталась помилка виходу";

            var tempUserIdStore = _userStore.UserId;
            _userStore.UserId = null;

            (var errorMessage, _) = await _apiService.PostAsync<object>("Auth", "logout", _userStore.RefreshToken, null, true);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _userStore.UserId = tempUserIdStore;
                return errorMessage;
            }

            _userStore.IsTokenTriedForLogin = true;

            _loginNavigationService.Navigate();

            return null;
        }
    }
}
