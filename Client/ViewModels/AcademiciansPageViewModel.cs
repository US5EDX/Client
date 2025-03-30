using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class AcademiciansPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private const int PAGESIZE = 50;

        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messageService;

        private readonly ObservableCollection<RoleInfo> _rolesInfo;
        private readonly ObservableCollection<UserFullInfo> _academicians;

        public IEnumerable<UserFullInfo> Academicians => _academicians;
        public IEnumerable<RoleInfo> RolesInfo => _rolesInfo;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextPageEnabled))]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        [NotifyPropertyChangedFor(nameof(IsPreviousPageEnabled))]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int _currentPage;

        [ObservableProperty]
        private int _totalPages;

        public int PageSize { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegistryOpen))]
        private AdminRegistryViewModel? _adminRegistry;

        public bool IsRegistryOpen => AdminRegistry is not null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLocked))]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAcademicianSelected))]
        [NotifyPropertyChangedFor(nameof(IsWorkerSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteLecturerCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetAcademicianPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToSelectedCommand))]
        private UserFullInfo? _selectedAcademician;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RoleFilter))]
        private RoleInfo? _selectedRole;

        public bool IsNotLocked => !IsWaiting;

        private string RoleFilter => SelectedRole?.RoleId == 0 ? string.Empty : $"&roleFilter={SelectedRole.RoleId}";

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsAcademicianSelected => SelectedAcademician is not null;
        public bool IsWorkerSelected => IsAcademicianSelected && SelectedAcademician.Role == 3;

        public bool IsNextPageEnabled => CurrentPage < TotalPages;
        public bool IsPreviousPageEnabled => CurrentPage > 1;

        public Func<object, string, bool> Filter { get; init; }

        public AcademiciansPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messageService = messageService;

            if (_userStore.Role != 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            _rolesInfo = new ObservableCollection<RoleInfo>()
            {
                new RoleInfo() { RoleId = 0, RoleName = "Усі" },
                new RoleInfo() {RoleId = 3, RoleName = "Викладачі"},
                new RoleInfo() { RoleId = 4, RoleName = "Студенти"}
            };

            _selectedRole = _rolesInfo[0];

            _academicians = new ObservableCollection<UserFullInfo>();

            WeakReferenceMessenger.Default.Register<WorkerUpdatedMessage>(this, Receive);

            PageSize = PAGESIZE;

            AdminRegistry = null;
            Filter = FilterWorkers;
        }

        public async Task LoadContentAsync()
        {
            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        private async Task LoadTotalPagesAsync()
        {
            (ErrorMessage, var totalSize) =
                await _apiService.GetAsync<int>("Academician",
                $"getCount?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{RoleFilter}", _userStore.AccessToken);

            if (HasErrorMessage)
                return;

            TotalPages = (int)Math.Ceiling((double)totalSize / PageSize);
            CurrentPage = 0;
        }

        private async Task LoadAcademiciansAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var academicians) =
                await _apiService.GetAsync<ObservableCollection<UserFullInfo>>("Academician",
                $"getAcademicians?pageNumber={page}&pageSize={PageSize}" +
                $"&facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{RoleFilter}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _academicians.Clear();

                    foreach (var academician in academicians ?? Enumerable.Empty<UserFullInfo>())
                        _academicians.Add(academician);

                    if (_academicians.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        partial void OnSelectedRoleChanged(RoleInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(LoadTotalPagesAsync);

            if (HasErrorMessage)
                return;

            await LoadAcademiciansAsync(1);
        }

        [RelayCommand]
        private void CloseModal()
        {
            AdminRegistry = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            AdminRegistry = new AdminRegistryViewModel(_userStore, _apiService, [null, _userStore.WorkerInfo.Faculty], CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private void OpenUpdateModal()
        {
            AdminRegistry = new AdminRegistryViewModel(_userStore, _apiService, [null, _userStore.WorkerInfo.Faculty],
                CloseModalCommand, SelectedAcademician);
        }

        public void Receive(object recipient, WorkerUpdatedMessage message)
        {
            UserFullInfo workerInfo = message.Value;

            if (IsWorkerSelected && SelectedAcademician.Id == workerInfo.Id)
            {
                SelectedAcademician.Email = workerInfo.Email;
                SelectedAcademician.Role = workerInfo.Role;
                SelectedAcademician.FullName = workerInfo.FullName;
                SelectedAcademician.Faculty = workerInfo.Faculty;
                SelectedAcademician.Department = workerInfo.Department;
                SelectedAcademician.Position = workerInfo.Position;
                SelectedAcademician = null;
                return;
            }

            _academicians.Add(workerInfo);
            SelectedAcademician = null;
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task DeleteLecturer()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити користувача {SelectedAcademician.FullName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Worker", $"deleteWorker/{SelectedAcademician.Id}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _academicians.Remove(SelectedAcademician);
                    SelectedAcademician = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsNextPageEnabled))]
        private async Task NextPage()
        {
            await LoadAcademiciansAsync(CurrentPage + 1);
        }

        [RelayCommand(CanExecute = nameof(IsPreviousPageEnabled))]
        private async Task PreviousPage()
        {
            await LoadAcademiciansAsync(CurrentPage - 1);
        }

        [RelayCommand(CanExecute = nameof(IsAcademicianSelected))]
        private async Task ResetAcademicianPassword()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити пароль користувача {SelectedAcademician.FullName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "User", $"resetPassword/{SelectedAcademician.Id}", null, _userStore.AccessToken);

                if (!HasErrorMessage)
                    SelectedAcademician = null;
            });
        }

        [RelayCommand(CanExecute = nameof(IsAcademicianSelected))]
        private async Task NavigateToSelected()
        {
            // not implemented
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private bool FilterWorkers(object worker, string filter)
        {
            if (worker is not UserFullInfo academicianInfo)
                return false;

            return academicianInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                academicianInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
