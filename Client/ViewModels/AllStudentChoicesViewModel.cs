using Client.Models;
using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace Client.ViewModels
{
    public partial class AllStudentChoicesViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly GroupInfoStore _groupInfoStore;
        private readonly StudentInfoStore _studentInfoStore;
        FrameNavigationService<GroupPageViewModel> _groupNavigation;
        FrameNavigationService<StudentYearChoicesViewModel> _studentYearChoicesNavigation;

        public ObservableCollection<StudentYearRecords> Records { get; init; }

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRecordSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenYearChoicesCommand))]
        private StudentYearRecords _selectedRecord;

        public string Header { get; set; }

        public bool IsRecordSelected => SelectedRecord is not null;

        public AllStudentChoicesViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            StudentInfoStore studentInfoStore, FrameNavigationService<GroupPageViewModel> groupNavigation,
            FrameNavigationService<StudentYearChoicesViewModel> studentYearChoicesNavigation)
        {
            _apiService = apiService;
            _userStore = userStore;
            _groupInfoStore = groupInfoStore;
            _studentInfoStore = studentInfoStore;

            _groupNavigation = groupNavigation;
            _studentYearChoicesNavigation = studentYearChoicesNavigation;

            Records = new ObservableCollection<StudentYearRecords>();
        }

        public async Task LoadContentAsync()
        {
            string? errorMessage = string.Empty;

            if (!_groupInfoStore.IsLoadedFromGroupsPage)
            {
                (errorMessage, var group) =
                    await _apiService.GetAsync<GroupInfo>
                    ("Group", $"getGroupById/{_studentInfoStore.GroupId}", _userStore.AccessToken);

                if (!string.IsNullOrEmpty(errorMessage))
                    throw new Exception(errorMessage);

                if (group is null)
                    throw new InvalidDataException("Не вдалось завантажити дані про групу");

                _groupInfoStore.GetInfoFromModel(group);
            }

            Header = _studentInfoStore.FullName;

            (errorMessage, var records) =
                    await _apiService.GetAsync<List<StudentYearRecords>>
                    ("Record", $"getByStudentIdAndCourse?studentId={_studentInfoStore.StudentId}&course={_groupInfoStore.Course}",
                    _userStore.AccessToken);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage);

            foreach (var record in records ?? Enumerable.Empty<StudentYearRecords>())
                Records.Add(record);
        }

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private void OpenYearChoices()
        {
            _studentInfoStore.SelectedYear = SelectedRecord.EduYear;
            _groupInfoStore.IsLoadedFromGroupsPage = true;
            _studentYearChoicesNavigation.RequestNavigation("YearChoices");
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _groupInfoStore.IsLoadedFromGroupsPage = true;
            _groupNavigation.RequestNavigation("Group");
        }
    }
}
