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
    public partial class AcademiciansPageViewModel : PaginationFrameViewModelBase
    {
        private readonly IMessageService _messageService;

        public ObservableCollection<UserFullInfo> Academicians { get; init; }
        public ObservableCollection<RoleInfo> RolesInfo { get; init; }

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

        public bool IsAcademicianSelected => SelectedAcademician is not null;

        public bool IsWorkerSelected => IsAcademicianSelected && SelectedAcademician.Role == 3;

        private string RoleFilter => SelectedRole?.RoleId == 0 ? string.Empty : $"&roleFilter={SelectedRole.RoleId}";

        public AcademiciansPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService) :
            base(apiService, userStore)
        {
            _messageService = messageService;

            if (_userStore.Role != 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            RolesInfo =
            [
                new RoleInfo() { RoleId = 0, RoleName = "Усі" },
                new RoleInfo() {RoleId = 3, RoleName = "Викладачі"},
                new RoleInfo() { RoleId = 4, RoleName = "Студенти"}
            ];

            _selectedRole = RolesInfo[0];

            Academicians = [];

            WeakReferenceMessenger.Default.Register<WorkerUpdatedMessage>(this, Receive);

            Filter = FilterAcademicians;
        }

        public override async Task LoadContentAsync()
        {
            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        public void Receive(object recipient, WorkerUpdatedMessage message)
        {
            var workerInfo = message.Value;

            if (IsWorkerSelected && SelectedAcademician.Id == workerInfo.Id)
                SelectedAcademician.UpdateInfo(workerInfo);
            else
                Academicians.Add(workerInfo);

            SelectedAcademician = null;
        }

        async partial void OnSelectedRoleChanged(RoleInfo? value) => await UpdateListingAsync();

        protected override async Task LoadDataAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var academicians) =
                await _apiService.GetAsync<List<UserFullInfo>>("Academician",
                $"getAcademicians?pageNumber={page}&pageSize={PageSize}" +
                $"&facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{RoleFilter}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Academicians.Clear();

                    foreach (var academician in academicians ?? Enumerable.Empty<UserFullInfo>())
                        Academicians.Add(academician);

                    if (Academicians.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(async () => await LoadTotalPagesAsync("Academician",
                $"getCount?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{RoleFilter}"));

            if (HasErrorMessage) return;

            await LoadDataAsync(1);
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal =
            new WorkerRegistryViewModel(_userStore, _apiService, [_userStore.WorkerInfo.Faculty], CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private void OpenUpdateModal() => SelectedModal =
            new WorkerRegistryViewModel(_userStore, _apiService,
                [_userStore.WorkerInfo.Faculty], CloseModalCommand, SelectedAcademician);

        [RelayCommand(CanExecute = nameof(IsWorkerSelected))]
        private async Task DeleteLecturer()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити користувача {SelectedAcademician.FullName}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Worker", $"deleteWorker/{SelectedAcademician.Id}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Academicians.Remove(SelectedAcademician);
                    SelectedAcademician = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsAcademicianSelected))]
        private async Task ResetAcademicianPassword()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити пароль користувача {SelectedAcademician.FullName}");

            if (!isOk) return;

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

        private bool FilterAcademicians(object academician, string filter)
        {
            if (academician is not UserFullInfo academicianInfo) return false;

            return academicianInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                academicianInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
