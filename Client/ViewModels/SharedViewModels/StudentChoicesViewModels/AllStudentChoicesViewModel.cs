using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels
{
    public partial class AllStudentChoicesViewModel : FrameBaseViewModel
    {
        private readonly GroupInfoStore _groupInfoStore;
        private readonly StudentInfoStore _studentInfoStore;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigation;
        private readonly FrameNavigationService<StudentYearChoicesViewModel> _studentYearChoicesNavigation;

        public List<StudentYearRecords> Records { get; init; }

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRecordSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenYearChoicesCommand))]
        private StudentYearRecords? _selectedRecord;

        public string Header { get; set; } = null!;

        public bool IsRecordSelected => SelectedRecord is not null;

        public AllStudentChoicesViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            StudentInfoStore studentInfoStore, FrameNavigationService<GroupPageViewModel> groupNavigation,
            FrameNavigationService<StudentYearChoicesViewModel> studentYearChoicesNavigation) : base(apiService, userStore)
        {
            _groupInfoStore = groupInfoStore;
            _studentInfoStore = studentInfoStore;

            _groupNavigation = groupNavigation;
            _studentYearChoicesNavigation = studentYearChoicesNavigation;

            Records = [];

            Header = studentInfoStore.FullName;
        }

        public override async Task LoadContentAsync()
        {
            await _groupInfoStore.LoadInfoAsync(_apiService, _studentInfoStore.GroupId, _userStore.AccessToken);

            (ErrorMessage, var records) =
                    await _apiService.GetAsync<List<StudentYearRecords>>
                    ("Record", $"getByStudentIdAndGroupId?studentId={_studentInfoStore.StudentId}&groupId={_groupInfoStore.GroupId}",
                    _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            Records.AddRange(records ?? Enumerable.Empty<StudentYearRecords>());
        }

        [RelayCommand(CanExecute = nameof(IsRecordSelected))]
        private void OpenYearChoices()
        {
            _studentInfoStore.SelectedYear = SelectedRecord.EduYear;
            _studentYearChoicesNavigation.RequestNavigation("YearChoices");
        }

        [RelayCommand]
        private void NavigateBack() => _groupNavigation.RequestNavigation("Group");
    }
}
