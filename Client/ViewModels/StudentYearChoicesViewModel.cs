using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class StudentYearChoicesViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messageService;
        private readonly GroupInfoStore _groupInfoStore;
        private readonly StudentInfoStore _studentInfoStore;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;

        private readonly ObservableCollection<RecordInfo> _records;

        public IEnumerable<RecordInfo> Records => _records;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRecordSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteRecordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateRecordStatusCommand))]
        private RecordInfo _selectedRecord;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsModalOpen))]
        private IPageViewModel? _selectedModal;

        public bool IsModalOpen => SelectedModal is not null;

        public string Header { get; init; }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        public bool IsRecordSelected => SelectedRecord is not null;

        public StudentYearChoicesViewModel(ApiService apiService, UserStore userStore, IMessageService messageService,
            GroupInfoStore groupInfoStore, StudentInfoStore studentInfoStore,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messageService = messageService;
            _groupInfoStore = groupInfoStore;
            _studentInfoStore = studentInfoStore;
            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;

            Header = $"{_studentInfoStore.FullName} {_studentInfoStore.SelectedYear}";

            _records = new ObservableCollection<RecordInfo>();

            WeakReferenceMessenger.Default.Register<RecordUpdatedMessage>(this, Receive);
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var records) =
                    await _apiService.GetAsync<List<RecordInfo>>
                    ("Record", $"getStudentYearRecords?studentId={_studentInfoStore.StudentId}&year={_studentInfoStore.SelectedYear}",
                    _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var record in records ?? Enumerable.Empty<RecordInfo>())
                _records.Add(record);
        }

        [RelayCommand]
        private void CloseModal()
        {
            SelectedModal = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            SelectedModal = new RecordRegistryViewModel(_apiService, _userStore, CloseModalCommand,
                _groupInfoStore.EduLevel, _studentInfoStore.StudentId, _studentInfoStore.SelectedYear);
        }

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private void OpenUpdateModal()
        {
            SelectedModal = new RecordRegistryViewModel(_apiService, _userStore, CloseModalCommand,
                _groupInfoStore.EduLevel, _studentInfoStore.StudentId, _studentInfoStore.SelectedYear, SelectedRecord);
        }

        public void Receive(object recipient, RecordUpdatedMessage message)
        {
            RecordInfo record = message.Value;

            if (IsRecordSelected && SelectedRecord.RecordId == record.RecordId)
            {
                SelectedRecord.Approved = record.Approved;
                SelectedRecord.DisciplineId = record.DisciplineId;
                SelectedRecord.DisciplineCode = record.DisciplineCode;
                SelectedRecord.DisciplineName = record.DisciplineName;
                SelectedRecord.Course = record.Course;
                SelectedRecord.EduLevel = record.EduLevel;
                SelectedRecord.Semester = record.Semester;
                SelectedRecord.SubscribersCount = record.SubscribersCount;
                SelectedRecord.IsOpen = record.IsOpen;
                SelectedRecord = null;
                return;
            }

            _records.Add(record);
            SelectedRecord = null;
        }

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
                    _records.Remove(SelectedRecord);
                    SelectedRecord = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private async Task UpdateRecordStatus()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "Record", $"updateRecordStatus/{SelectedRecord.RecordId}", null, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedRecord.Approved = !SelectedRecord.Approved;
                    SelectedRecord = null;
                }
            });
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _groupInfoStore.IsLoadedFromGroupsPage = true;
            _allStudentCohicesNavigationService.RequestNavigation("AllChoices");
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
