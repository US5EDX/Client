using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class AdminRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private CancellationTokenSource _cts;

        private readonly UserStore _userStore;
        private readonly ApiService _apiService;
        private readonly ObservableCollection<FacultyInfo> _facultiesInfo;
        private readonly ObservableCollection<RoleInfo> _rolesInfo;
        private readonly ObservableCollection<GroupShortInfo> _groupsInfo;

        public IEnumerable<FacultyInfo> Faculties => _facultiesInfo;
        public IEnumerable<RoleInfo> Roles => _rolesInfo;
        public IEnumerable<GroupShortInfo> Groups => _groupsInfo;

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
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        private GroupShortInfo? _group;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        private string _groupName;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        private bool _isAddMode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateWorkerCommand))]
        private bool _isCurator;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !string.IsNullOrEmpty(Email) &&
            !string.IsNullOrEmpty(FullName) &&
            !string.IsNullOrEmpty(Department) &&
            !string.IsNullOrEmpty(Position) &&
            Role is not null &&
            Faculty is not null &&
            (!IsAdministratorView ||
            (IsAdministratorView &&
            (!IsCurator ||
            (IsCurator && Group is not null)))) &&
            !HasErrors;

        public bool IsAdministratorView { get; init; }

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public AdminRegistryViewModel(UserStore userStore, ApiService apiService, IEnumerable<FacultyInfo> facultiesInfo,
            IRelayCommand closeCommand, UserFullInfo? WorkerInfo = null)
        {
            _userStore = userStore;
            _apiService = apiService;

            if (_userStore.Role > 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            IsAdministratorView = _userStore.Role == 2;

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

                _group = WorkerInfo?.Group;
            }
            else
            {
                Header = WorkerInfo is null ? "Додати викладача" : "Редагувати інформацію про викладача";

                _groupsInfo = new ObservableCollection<GroupShortInfo>();

                _rolesInfo.Add(new RoleInfo { RoleId = 3, RoleName = "Викладач" });
                _role = _rolesInfo[0];

                _groupsInfo = new ObservableCollection<GroupShortInfo>();

                if (WorkerInfo is not null && WorkerInfo.Group is not null)
                {
                    _isCurator = true;
                    _groupsInfo.Add(WorkerInfo.Group);
                    Group = _groupsInfo[0];
                    _groupName = Group.GroupCode;
                }
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

        partial void OnGroupNameChanged(string value)
        {
            if (Group is not null && Group.GroupCode == value)
                return;

            if (value.Length < 3)
            {
                _groupsInfo.Clear();
                Group = null;
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                await Task.Delay(500);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _groupsInfo.Clear();
                    _groupsInfo.Add(new GroupShortInfo { GroupCode = "Пошук..." });
                    IsLoading = true;
                });

                var result = await LoadGroupsFromDatabase(value);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _groupsInfo.Clear();
                    foreach (var item in result)
                        _groupsInfo.Add(item);
                    IsLoading = false;
                });
            }, token);
        }

        private async Task<IEnumerable<GroupShortInfo>> LoadGroupsFromDatabase(string searchFilter)
        {
            (ErrorMessage, var groups) =
                await _apiService.GetAsync<ObservableCollection<GroupShortInfo>>("Group",
                $"getGroupsByCodeSearch?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                $"&codeFilter={searchFilter}", _userStore.AccessToken);

            return groups ?? Enumerable.Empty<GroupShortInfo>();
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddWorker()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await ExecuteWithWaiting(async () =>
            {
                var newWorker = new UserFullInfo
                {
                    Email = Email,
                    Role = Role.RoleId,
                    FullName = FullName,
                    Faculty = Faculty,
                    Department = Department,
                    Position = Position,
                    Group = !IsAdministratorView ? Group : (IsCurator ? null : Group)
                };

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
                var updatedWorker = new UserFullInfo
                {
                    Id = _id,
                    Email = Email,
                    Role = Role.RoleId,
                    FullName = FullName,
                    Faculty = Faculty,
                    Department = Department,
                    Position = Position,
                    Group = !IsAdministratorView ? Group : (IsCurator ? Group : null)
                };

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
    }
}
