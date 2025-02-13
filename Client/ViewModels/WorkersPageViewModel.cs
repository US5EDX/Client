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
    public partial class WorkersPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messageService;

        private readonly ObservableCollection<FacultyInfo> _facultiesInfo;
        private readonly ObservableCollection<WorkerFullInfo> _workers;

        public IEnumerable<WorkerFullInfo> Workers => _workers;
        public IEnumerable<FacultyInfo> FacultiesInfo => _facultiesInfo;

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
        [NotifyPropertyChangedFor(nameof(IsWorkerSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetWorkerPasswordCommand))]
        private WorkerFullInfo? _selectedWorker;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FacultyFilter))]
        private FacultyInfo? _selectedFaculty;

        public bool IsNotLocked => !IsWaiting;

        private string FacultyFilter => SelectedFaculty?.FacultyId == 0 ? string.Empty : $"facultyFilter={SelectedFaculty.FacultyId}";

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsWorkerSelected => SelectedWorker is not null;

        public bool IsNextPageEnabled => CurrentPage < TotalPages;
        public bool IsPreviousPageEnabled => CurrentPage > 1;

        public Func<object, string, bool> Filter { get; init; }

        public WorkersPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messageService = messageService;

            _facultiesInfo = new ObservableCollection<FacultyInfo>() { new FacultyInfo() { FacultyId = 0, FacultyName = "Усі" } };
            _selectedFaculty = _facultiesInfo[0];
            _workers = new ObservableCollection<WorkerFullInfo>();

            WeakReferenceMessenger.Default.Register<WorkerUpdatedMessage>(this, Receive);

            PageSize = 2;

            AdminRegistry = null;
            Filter = FilterWorkers;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var faculties) =
                await _apiService.GetAsync<ObservableCollection<FacultyInfo>>("Faculty", "getFaculties", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var faculty in faculties ?? Enumerable.Empty<FacultyInfo>())
                _facultiesInfo.Add(faculty);

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        private async Task LoadTotalPagesAsync()
        {
            (ErrorMessage, var totalSize) =
                await _apiService.GetAsync<int>("Worker", $"getCount?{FacultyFilter}", _userStore.AccessToken);

            if (HasErrorMessage)
                return;

            var test = Math.Ceiling((double)totalSize / PageSize);

            TotalPages = (int)Math.Ceiling((double)totalSize / PageSize);
            CurrentPage = 0;
        }

        private async Task LoadWorkersAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var workers) =
                await _apiService.GetAsync<ObservableCollection<WorkerFullInfo>>("Worker",
                $"getWorkers?pageNumber={page}&pageSize={PageSize}&{FacultyFilter}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _workers.Clear();

                    foreach (var worker in workers ?? Enumerable.Empty<WorkerFullInfo>())
                        _workers.Add(worker);

                    if (_workers.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        partial void OnSelectedFacultyChanged(FacultyInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(LoadTotalPagesAsync);

            if (HasErrorMessage)
                return;

            await LoadWorkersAsync(1);
        }

        [RelayCommand]
        private void CloseModal()
        {
            AdminRegistry = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            AdminRegistry = new AdminRegistryViewModel(_userStore, _apiService, FacultiesInfo, CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private void OpenUpdateModal()
        {
            AdminRegistry = new AdminRegistryViewModel(_userStore, _apiService, FacultiesInfo, CloseModalCommand, SelectedWorker);
        }

        public void Receive(object recipient, WorkerUpdatedMessage message)
        {
            WorkerFullInfo workerInfo = message.Value;

            if (IsWorkerSelected && SelectedWorker.Id == workerInfo.Id)
            {
                SelectedWorker.Email = workerInfo.Email;
                SelectedWorker.Role = workerInfo.Role;
                SelectedWorker.FullName = workerInfo.FullName;
                SelectedWorker.Faculty = workerInfo.Faculty;
                SelectedWorker.Department = workerInfo.Department;
                SelectedWorker.Position = workerInfo.Position;
                SelectedWorker = null;
                return;
            }

            _workers.Add(workerInfo);
            SelectedWorker = null;
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task DeleteWorker()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити користувача {SelectedWorker.FullName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Worker", $"deleteWorker/{SelectedWorker.Id}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _workers.Remove(SelectedWorker);
                    SelectedWorker = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsNextPageEnabled))]
        private async Task NextPage()
        {
            await LoadWorkersAsync(CurrentPage + 1);
        }

        [RelayCommand(CanExecute = nameof(IsPreviousPageEnabled))]
        private async Task PreviousPage()
        {
            await LoadWorkersAsync(CurrentPage - 1);
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task ResetWorkerPassword()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити пароль користувача {SelectedWorker.FullName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "User", $"resetPassword/{SelectedWorker.Id}", null, _userStore.AccessToken);

                if (!HasErrorMessage)
                    SelectedWorker = null;
            });
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
            if (worker is not WorkerFullInfo workerInfo)
                return false;

            return workerInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                workerInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
