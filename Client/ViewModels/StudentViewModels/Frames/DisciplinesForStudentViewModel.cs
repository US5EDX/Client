using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class DisciplinesForStudentViewModel : PaginationFrameViewModelBase
    {
        public HoldingInfo Holding { get; set; } = null!;

        public int NextEduYear => Holding.EduYear + 1;

        public List<CatalogTypeInfo> CatalogTypes { get; init; }
        public List<FacultyInfo> Faculties { get; init; }
        public List<SemesterInfo> Semesters { get; init; }

        public ObservableCollection<DisciplineInfoForStudent> Disciplines { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDisciplineSelected))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToFullInfoCommand))]
        private DisciplineInfoForStudent? _selectedDiscipline;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CatalogFilter))]
        private CatalogTypeInfo? _selectedCatalog;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FacultyFilter))]
        private FacultyInfo? _selectedFaculty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemesterFilter))]
        private SemesterInfo? _selectedSemester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBlocked))]
        private string? _blockedMessage = default!;

        public bool IsBlocked => !string.IsNullOrEmpty(BlockedMessage);

        public bool IsDisciplineSelected => SelectedDiscipline is not null;

        private string CatalogFilter => $"&catalogFilter={SelectedCatalog?.CatalogType ?? 1}";

        private string FacultyFilter => SelectedFaculty?.FacultyId == 0 ? string.Empty : $"&facultyFilter={SelectedFaculty.FacultyId}";

        private string CourseFilter => $"&courseFilter={_userStore.StudentInfo.Group.Course +
            _userStore.StudentInfo.Group.ChoiceDifference +
            ((_userStore.StudentInfo.Group.AdmissionYear == Holding.EduYear
            && _userStore.StudentInfo.Group.HasEnterChoise) ? 0 : 1)}";

        private string SemesterFilter => $"&semesterFilter={SelectedSemester?.SemesterId ?? 0}";

        public DisciplinesForStudentViewModel(ApiService apiService, UserStore userStore) : base(apiService, userStore)
        {
            if (_userStore.StudentInfo is null)
                throw new Exception("Доступ обмежено");

            if (_userStore.StudentInfo.Group.Course == _userStore.StudentInfo.Group.DurationOfStudy ||
                _userStore.StudentInfo.Group.Course == 0)
            {
                BlockedMessage = "Для вашої групи наразі не запланований вибір дисциплін";
                return;
            }

            CatalogTypes =
            [
                new CatalogTypeInfo() { CatalogType = 1, CatalogName = "УВК"},
                new CatalogTypeInfo() { CatalogType = 2, CatalogName = "ФВК"},
            ];

            _selectedCatalog = CatalogTypes[0];

            Faculties =
            [
                new FacultyInfo() { FacultyId = 0, FacultyName = "Всі факультети" },
            ];

            _selectedFaculty = Faculties[0];

            Semesters =
            [
                new SemesterInfo() { SemesterId = 0, SemesterName = "Обидва" },
                new SemesterInfo() { SemesterId = 1, SemesterName = "Осінній" },
                new SemesterInfo() { SemesterId = 2, SemesterName = "Весняний" },
            ];

            _selectedSemester = Semesters[0];

            Disciplines = [];

            Filter = FilterDisciplines;
        }

        public override async Task LoadContentAsync()
        {
            if (IsBlocked)
                return;

            (ErrorMessage, Holding) =
                await _apiService.GetAsync<HoldingInfo>("Holding",
                $"getLastHolding", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            if (Holding.EduYear == _userStore.StudentInfo.Group.AdmissionYear && !_userStore.StudentInfo.Group.HasEnterChoise)
            {
                BlockedMessage = "Для вашої групи наразі не запланований вибір дисциплін";
                return;
            }

            (ErrorMessage, var faculties) =
                await _apiService.GetAsync<List<FacultyInfo>>
                ("Faculty", $"getFaculties", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            Faculties.AddRange(faculties ?? Enumerable.Empty<FacultyInfo>());

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        async partial void OnSelectedCatalogChanged(CatalogTypeInfo? value) => await UpdateListingAsync();

        async partial void OnSelectedFacultyChanged(FacultyInfo? value) => await UpdateListingAsync();

        async partial void OnSelectedSemesterChanged(SemesterInfo? value) => await UpdateListingAsync();

        protected override async Task LoadDataAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var disciplines) =
                await _apiService.GetAsync<List<DisciplineInfoForStudent>>("Discipline",
                $"getDisciplinesForStudent/{page}/{PageSize}" +
                $"?eduLevel={_userStore.StudentInfo.Group.EduLevel}&holding={Holding.EduYear}" +
                $"{CourseFilter}{CatalogFilter}{SemesterFilter}{FacultyFilter}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Disciplines.Clear();

                    foreach (var discipline in disciplines ?? Enumerable.Empty<DisciplineInfoForStudent>())
                        Disciplines.Add(discipline);

                    if (Disciplines.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(async () => await LoadTotalPagesAsync("Discipline",
                $"getCountForStudent?eduLevel={_userStore.StudentInfo.Group.EduLevel}&holding={Holding.EduYear}" +
                $"{CourseFilter}{CatalogFilter}{SemesterFilter}{FacultyFilter}"));

            if (HasErrorMessage) return;

            await LoadDataAsync(1);
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private async Task NavigateToFullInfo()
        {
            DisciplineFullInfo? discipline = null!;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, discipline) =
                await _apiService.GetAsync<DisciplineFullInfo>("Discipline",
                $"getDisciplineById/{SelectedDiscipline.DisciplineId}", _userStore.AccessToken);
            });

            if (HasErrorMessage) return;

            SelectedModal = new DisciplinePreviewViewModel(CloseModalCommand, discipline);
        }

        private bool FilterDisciplines(object discipline, string filter)
        {
            if (discipline is not DisciplineInfoForStudent disciplineInfo) return false;

            return disciplineInfo.DisciplineName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                disciplineInfo.DisciplineCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
