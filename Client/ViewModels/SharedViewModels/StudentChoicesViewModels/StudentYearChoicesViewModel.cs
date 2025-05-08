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
    public partial class StudentYearChoicesViewModel : FrameBaseViewModelWithModal
    {
        private readonly IMessageService _messageService;
        private readonly GroupInfoStore _groupInfoStore;
        private readonly StudentInfoStore _studentInfoStore;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;

        public ObservableCollection<RecordInfo> Records { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRecordSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteRecordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateRecordStatusCommand))]
        private RecordInfo? _selectedRecord;

        public string Header { get; init; }

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        public bool IsRecordSelected => SelectedRecord is not null;

        public StudentYearChoicesViewModel(ApiService apiService, UserStore userStore, IMessageService messageService,
            GroupInfoStore groupInfoStore, StudentInfoStore studentInfoStore,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService) :
            base(apiService, userStore)
        {
            _messageService = messageService;
            _groupInfoStore = groupInfoStore;
            _studentInfoStore = studentInfoStore;
            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;

            Header = $"{_studentInfoStore.FullName} {_studentInfoStore.SelectedYear}";

            Records = [];

            WeakReferenceMessenger.Default.Register<RecordUpdatedMessage>(this, Receive);
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var records) =
                    await _apiService.GetAsync<List<RecordInfo>>
                    ("Record", $"getStudentYearRecords?studentId={_studentInfoStore.StudentId}&year={_studentInfoStore.SelectedYear}",
                    _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var record in records ?? Enumerable.Empty<RecordInfo>())
                Records.Add(record);
        }

        public void Receive(object recipient, RecordUpdatedMessage message)
        {
            RecordInfo record = message.Value;

            if (IsRecordSelected && SelectedRecord.RecordId == record.RecordId)
                SelectedRecord.UpdateInfo(record);
            else
                Records.Add(record);

            SelectedRecord = null;
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new RecordRegistryViewModel(_apiService, _userStore, CloseModalCommand,
                _groupInfoStore.EduLevel, _studentInfoStore.StudentId, _studentInfoStore.SelectedYear);

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private void OpenUpdateModal() => SelectedModal = new RecordRegistryViewModel(_apiService, _userStore, CloseModalCommand,
                _groupInfoStore.EduLevel, _studentInfoStore.StudentId, _studentInfoStore.SelectedYear, SelectedRecord);

        [RelayCommand]
        private void NavigateBack() => _allStudentCohicesNavigationService.RequestNavigation("AllChoices");

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private async Task DeleteRecord()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити запис на {SelectedRecord.DisciplineName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Record", $"deleteRecord/{SelectedRecord.RecordId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Records.Remove(SelectedRecord);
                    SelectedRecord = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private async Task UpdateRecordStatus(byte status)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "Record", $"updateRecordStatus/{SelectedRecord.RecordId}", status, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedRecord.Approved = status;
                    SelectedRecord = null;
                }
            });
        }
    }
}
