using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class WorkersPageViewModel : PaginationFrameViewModelBase
    {
        private readonly IMessageService _messageService;

        public List<FacultyInfo> FacultiesInfo { get; init; }

        public ObservableCollection<UserFullInfo> Workers { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsWorkerSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteWorkerCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetWorkerPasswordCommand))]
        private UserFullInfo? _selectedWorker;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FacultyFilter))]
        private FacultyInfo? _selectedFaculty;

        public bool IsWorkerSelected => SelectedWorker is not null;

        private string FacultyFilter => SelectedFaculty?.FacultyId == 0 ? string.Empty : $"facultyFilter={SelectedFaculty.FacultyId}";

        public WorkersPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService) :
            base(apiService, userStore)
        {
            _messageService = messageService;

            FacultiesInfo = [new FacultyInfo() { FacultyId = 0, FacultyName = "Усі" }];
            _selectedFaculty = FacultiesInfo[0];
            Workers = [];

            WeakReferenceMessenger.Default.Register<WorkerUpdatedMessage>(this, Receive);

            Filter = FilterWorkers;
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var faculties) =
                await _apiService.GetAsync<ObservableCollection<FacultyInfo>>("Faculty", "getFaculties", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            FacultiesInfo.AddRange(faculties ?? Enumerable.Empty<FacultyInfo>());

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        public void Receive(object recipient, WorkerUpdatedMessage message)
        {
            UserFullInfo workerInfo = message.Value;

            if (IsWorkerSelected && SelectedWorker.Id == workerInfo.Id)
                SelectedWorker.UpdateInfo(workerInfo);
            else
                Workers.Add(workerInfo);

            SelectedWorker = null;
        }

        async partial void OnSelectedFacultyChanged(FacultyInfo? value) => await UpdateListingAsync();

        protected override async Task LoadDataAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var workers) =
                await _apiService.GetAsync<ObservableCollection<UserFullInfo>>("Worker",
                $"getWorkers?pageNumber={page}&pageSize={PageSize}&{FacultyFilter}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Workers.Clear();

                    foreach (var worker in workers ?? Enumerable.Empty<UserFullInfo>())
                        Workers.Add(worker);

                    if (Workers.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(async () => await LoadTotalPagesAsync("Worker", $"getCount?{FacultyFilter}"));

            if (HasErrorMessage) return;

            await LoadDataAsync(1);
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new WorkerRegistryViewModel(_userStore, _apiService,
            FacultiesInfo.Skip(1), CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private void OpenUpdateModal() => SelectedModal = new WorkerRegistryViewModel(_userStore, _apiService,
            FacultiesInfo.Skip(1), CloseModalCommand, SelectedWorker);

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task DeleteWorker()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити користувача {SelectedWorker.FullName}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Worker", $"deleteWorker/{SelectedWorker.Id}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Workers.Remove(SelectedWorker);
                    SelectedWorker = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task ResetWorkerPassword()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити пароль користувача {SelectedWorker.FullName}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "User", $"resetPassword/{SelectedWorker.Id}", null, _userStore.AccessToken);

                if (!HasErrorMessage) SelectedWorker = null;
            });
        }

        private bool FilterWorkers(object worker, string filter)
        {
            if (worker is not UserFullInfo workerInfo) return false;

            return workerInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                workerInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
