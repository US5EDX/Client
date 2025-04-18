﻿using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class AdminRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;
        private readonly ObservableCollection<FacultyInfo> _facultiesInfo;
        private readonly ObservableCollection<RoleInfo> _rolesInfo;

        public IEnumerable<FacultyInfo> Faculties => _facultiesInfo;
        public IEnumerable<RoleInfo> Roles => _rolesInfo;

        [ObservableProperty]
        private string _header;

        private readonly string? _id;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        [Required]
        [EmailAddress]
        private string _email;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        private RoleInfo? _role;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        [Required]
        [Length(2, 150)]
        private string _fullName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        private FacultyInfo? _faculty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        [Required]
        [Length(2, 255)]
        private string _department;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        [Required]
        [Length(2, 100)]
        private string _position;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        private bool _isAddMode;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !string.IsNullOrEmpty(Email) &&
            !string.IsNullOrEmpty(FullName) &&
            !string.IsNullOrEmpty(Department) &&
            !string.IsNullOrEmpty(Position) &&
            Role is not null &&
            Faculty is not null &&
            !HasErrors;

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public AdminRegistryViewModel(UserStore userStore, ApiService apiService, IEnumerable<FacultyInfo> facultiesInfo,
            IRelayCommand closeCommand, UserFullInfo? WorkerInfo = null)
        {
            _userStore = userStore;
            _apiService = apiService;

            if (_userStore.Role > 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            _facultiesInfo = [.. facultiesInfo.Skip(1)];

            _rolesInfo = new ObservableCollection<RoleInfo>();

            if (_userStore.Role == 1)
            {
                Header = WorkerInfo is null ? "Додати адміністратора" : "Редагувати інформацію про адмістратора";

                _rolesInfo.Add(new RoleInfo { RoleId = 2, RoleName = "Адміністратор" });

                if (WorkerInfo is not null)
                    _rolesInfo.Add(new RoleInfo { RoleId = 3, RoleName = "Викладач" });

                int role = WorkerInfo?.Role ?? 2;
                _role = _rolesInfo[role - 2];
            }
            else
            {
                Header = WorkerInfo is null ? "Додати викладача" : "Редагувати інформацію про викладача";

                _rolesInfo.Add(new RoleInfo { RoleId = 3, RoleName = "Викладач" });
                _role = _rolesInfo[0];
            }

            CloseCommand = closeCommand;
            SubmitCommand = WorkerInfo is null ? AddWorkerCommand : UpdateWorkerCommand;

            IsAddMode = WorkerInfo is null;

            _id = WorkerInfo?.Id;
            _email = WorkerInfo?.Email ?? string.Empty;
            _fullName = WorkerInfo?.FullName ?? string.Empty;
            _faculty = _facultiesInfo.FirstOrDefault(f => f.FacultyId == WorkerInfo?.Faculty.FacultyId);
            _department = WorkerInfo?.Department ?? string.Empty;
            _position = WorkerInfo?.Position ?? string.Empty;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddWorker()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newWorker = InithializeUser();

                (ErrorMessage, var addedWorker) =
                    await _apiService.PostAsync<UserFullInfo>("Worker", "addWorker", newWorker, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedWorker);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateWorker()
        {
            await ExecuteWithWaiting(async () =>
            {
                var updatedWorker = InithializeUser();

                (ErrorMessage, _) =
                    await _apiService.PutAsync<UserFullInfo>("Worker", "updateWorker", updatedWorker, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedWorker);
            });
        }

        private void OnSubmitAccepted(UserFullInfo workerInfo)
        {
            WeakReferenceMessenger.Default.Send(new WorkerUpdatedMessage(workerInfo));
            CloseCommand.Execute(null);
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private UserFullInfo InithializeUser()
        {
            return new UserFullInfo
            {
                Id = _id,
                Email = Email,
                Role = Role.RoleId,
                FullName = FullName,
                Faculty = Faculty,
                Department = Department,
                Position = Position,
            };
        }
    }
}
