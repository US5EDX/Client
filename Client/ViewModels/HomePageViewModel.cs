using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace Client.ViewModels
{
    public partial class HomePageViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;

        [ObservableProperty]
        private bool _isUpdateOpen;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _oldPassword;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _newPassword;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanUpdatePassword))]
        [NotifyCanExecuteChangedFor(nameof(UpdatePasswordCommand))]
        private string _confirmPassword;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSubmitErrorMessage))]
        private string? _submitErrorMessage = default!;

        public bool HasSubmitErrorMessage => !string.IsNullOrEmpty(SubmitErrorMessage);

        public bool CanUpdatePassword =>
            !string.IsNullOrEmpty(OldPassword) &&
            !string.IsNullOrEmpty(NewPassword) &&
            !string.IsNullOrEmpty(ConfirmPassword) &&
            Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?!.*(.)\1)[a-zA-Z\d!""#$%&'()*+,\-./:;<=>?\@[\\\]^_{|}~]{8,}$") &&
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

        public HomePageViewModel(UserStore userStore, ApiService apiService)
        {
            _userStore = userStore;
            _apiService = apiService;
        }

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
                SubmitErrorMessage = "Новий пароль не може бути таким самим, як старий";
                return;
            }

            SubmitErrorMessage = string.Empty;
            IsWaiting = true;

            (SubmitErrorMessage, _) =
                await _apiService.PutAsync<object>("User", "updatePassword", new UpdatePasswordInfo()
                {
                    UserId = _userStore.UserId,
                    OldPassword = OldPassword,
                    NewPassword = NewPassword
                }, _userStore.AccessToken);

            if (!HasSubmitErrorMessage)
                OpenUpdate();

            IsWaiting = false;
        }
    }
}
