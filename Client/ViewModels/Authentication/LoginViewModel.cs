using Client.Models;
using Client.Services;
using Client.Storages;
using Client.Stores;
using Client.ViewModels.Base;
using Client.ViewModels.NavigationViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json.Nodes;

namespace Client.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly SuccsefulLoginViewModel _successfulLogin;

        [ObservableProperty]
        private string? _email;

        [ObservableProperty]
        private string? _password;

        public LoginViewModel(ApiService apiService, SuccsefulLoginViewModel successfulLogin, UserStore userStore)
            : base(apiService, userStore)
        {
            _successfulLogin = successfulLogin;

            Task.Run(TryAutoLogin);
        }

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = Validation.Validation.ValidateLoginData(Email, Password);

            if (HasErrorMessage) return;

            await ExecuteWithWaiting(async () =>
            {
                bool isSuccess = await ProccessLoginApiCall("Auth", "login", new LoginInfo()
                {
                    Email = Email,
                    Password = Password
                });

                if (isSuccess)
                    _successfulLogin.NavigateBasedOnRole();
            });
        }

        private async Task TryAutoLogin()
        {
            if (_userStore.IsTokenTriedForLogin)
                return;

            var refreshToken = await TokenStorage.LoadTokenAsync();

            if (refreshToken == null || refreshToken.Length < 32)
            {
                TokenStorage.DeleteToken();
                _userStore.IsTokenTriedForLogin = true;
                return;
            }

            await ExecuteWithWaiting(async () =>
            {
                bool isSuccess = await ProccessLoginApiCall("Auth", "autologin", refreshToken);

                _userStore.IsTokenTriedForLogin = true;

                if (isSuccess)
                    _successfulLogin.NavigateBasedOnRole();
            });
        }

        private async Task<bool> ProccessLoginApiCall(string nav, string endpoint, object requestObject)
        {
            (ErrorMessage, var responseObject) = await _apiService.PostAsync<JsonObject>(nav, endpoint, requestObject, null, true);

            if (HasErrorMessage) return false;

            _userStore.LoadUserStoreFromJson(responseObject);
            await TokenStorage.SaveTokenAsync(_userStore.RefreshToken);

            return true;
        }
    }
}
