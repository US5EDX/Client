using Client.API;
using Client.Handlers;
using Client.Models;
using Client.Storages;
using Client.Stores;
using Client.ViewModels.Interfaces;
using Client.ViewModels.NavigationViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly Endpoints _endpoints;
        private readonly SuccsefulLoginViewModel _successfulLogin;
        private readonly UserStore _userStore;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string _errorMessage = default!;

        [ObservableProperty]
        private bool _isLoading;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public LoginViewModel(Endpoints endpoints, SuccsefulLoginViewModel successfulLogin, UserStore userStore)
        {
            _endpoints = endpoints;
            _successfulLogin = successfulLogin;
            _userStore = userStore;

            Task.Run(async () => await TryAutoLogin());
        }

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = Validation.Validation.ValidateLoginData(Email, Password);

            if (HasErrorMessage)
                return;

            IsLoading = true;

            bool result = await ProccessLoginApiCall("Auth", "login", new LoginInfo()
            {
                Email = Email,
                Password = Password
            },
            "Неправильна пошта або пароль");

            if (result)
                _successfulLogin.NavigateBasedOnRole();

            IsLoading = false;
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

            IsLoading = true;

            bool result = await ProccessLoginApiCall("Auth", "autologin", refreshToken, "Сталась помилка автоматичного входу");

            if (result)
                _successfulLogin.NavigateBasedOnRole();

            _userStore.IsTokenTriedForLogin = true;
            IsLoading = false;
        }

        private async Task<bool> ProccessLoginApiCall(string nav, string endpoint, object requestObject, string unauthorizedErrorMessage)
        {
            var response = await _endpoints.PostCall(nav, endpoint, requestObject);

            ErrorMessage = ApiResponseStatusCodeHandler.HandleApiResponse(response, unauthorizedErrorMessage);

            if (HasErrorMessage)
                return false;

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonObject>(responseContent);

            if (responseObject == null)
            {
                ErrorMessage = "Некоректна відповідь від сервера";
                return false;
            }

            _userStore.LoadUserStoreFromJson(responseObject);
            await TokenStorage.SaveTokenAsync(_userStore.RefreshToken);

            return true;
        }
    }
}
