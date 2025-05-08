using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace Client.ViewModels
{
    public partial class HomePageViewModel(UserStore userStore, ApiService apiService) : ViewModelBase(apiService, userStore)
    {
        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?!.*(.)\1)[a-zA-Z\d!""#$%&'()*+,\-./:;<=>?\@[\\\]^_{|}~]{8,}$")]
        private static partial Regex PasswordValidationRegex();

        [ObservableProperty]
        private bool _isUpdateOpen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _oldPassword = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _newPassword = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _confirmPassword = null!;

        public bool CanUpdatePassword =>
            !string.IsNullOrEmpty(OldPassword) &&
            !string.IsNullOrEmpty(NewPassword) &&
            !string.IsNullOrEmpty(ConfirmPassword) &&
            PasswordValidationRegex().IsMatch(NewPassword) &&
            NewPassword == ConfirmPassword;

        public string HelloMessage =>
            "Доброго дня, " + (_userStore.WorkerInfo?.FullName ?? _userStore.StudentInfo?.FullName ?? "Супер Адмін");

        public string Email => _userStore.Email;

        public string FacultyName =>
            _userStore.StudentInfo?.Faculty.FacultyName ?? _userStore.WorkerInfo?.Faculty.FacultyName ?? "Адміністрація";

        public string WorkPlace =>
            _userStore.WorkerInfo?.Department ?? _userStore.StudentInfo?.Group.GroupCode ?? "Адміністрація";

        public string Position =>
            _userStore.WorkerInfo?.Position ??
            (_userStore.StudentInfo is null ? "Адміністрація" : $"студент-{_userStore.StudentInfo?.Group.Course}");

        [RelayCommand]
        private void OpenUpdate()
        {
            IsUpdateOpen = !IsUpdateOpen;

            if (!IsUpdateOpen)
            {
                OldPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
        }

        [RelayCommand(CanExecute = nameof(CanUpdatePassword))]
        private async Task UpdatePassword()
        {
            if (OldPassword == NewPassword)
            {
                ErrorMessage = "Новий пароль не може бути таким самим, як старий";
                return;
            }

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                await _apiService.PutAsync<object>("User", "updatePassword", new UpdatePasswordInfo()
                {
                    UserId = _userStore.UserId,
                    OldPassword = OldPassword,
                    NewPassword = NewPassword
                }, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OpenUpdate();
            });
        }
    }
}
