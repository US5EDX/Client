using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class DisciplinesForStudentViewModel : ObservableRecipient, IFrameViewModel
    {
        private const int PAGESIZE = 50;

        private readonly ApiService _apiService;
        private readonly UserStore _userStore;

        private string CatalogFilter => $"&catalogFilter={SelectedCatalog?.CatalogType ?? 1}";

        private string FacultyFilter => SelectedFaculty?.FacultyId == 0 ? string.Empty : $"&facultyFilter={SelectedFaculty.FacultyId}";

        private string CourseFilter => $"&courseFilter={_userStore.StudentInfo.Group.Course +
            _userStore.StudentInfo.Group.ChoiceDifference +
            ((_userStore.StudentInfo.Group.Course == 1 && _userStore.StudentInfo.Group.HasEnterChoise) ? 0 : 1)}";

        private string SemesterFilter => $"&semesterFilter={SelectedSemester?.SemesterId ?? 0}";

        public HoldingInfo Holding { get; set; }

        public int NextEduYear => Holding.EduYear + 1;

        public List<CatalogTypeInfo> CatalogTypes { get; init; }
        public List<FacultyInfo> Faculties { get; init; }
        public List<SemesterInfo> Semesters { get; init; }

        public ObservableCollection<DisciplineInfoForStudent> Disciplines { get; init; }

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
        [NotifyPropertyChangedFor(nameof(IsModalOpen))]
        private IPageViewModel? _selectedModal;

        public bool IsModalOpen => SelectedModal is not null;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLocked))]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

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

        public bool IsNotLocked => !IsWaiting;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsDisciplineSelected => SelectedDiscipline is not null;

        public bool IsNextPageEnabled => CurrentPage < TotalPages;
        public bool IsPreviousPageEnabled => CurrentPage > 1;

        public Func<object, string, bool> Filter { get; init; }

        public DisciplinesForStudentViewModel(ApiService apiService, UserStore userStore)
        {
            _apiService = apiService;
            _userStore = userStore;

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
                new() { CatalogType = 1, CatalogName = "УВК"},
                new() { CatalogType = 2, CatalogName = "ФВК"},
            ];

            _selectedCatalog = CatalogTypes[0];

            Faculties =
            [
                new() { FacultyId = 0, FacultyName = "Всі факультети" },
            ];

            _selectedFaculty = Faculties[0];

            Semesters =
            [
                new() { SemesterId = 0, SemesterName = "Обидва" },
                new() { SemesterId = 1, SemesterName = "Осінній" },
                new() { SemesterId = 2, SemesterName = "Весняний" },
            ];

            _selectedSemester = Semesters[0];

            Disciplines = new ObservableCollection<DisciplineInfoForStudent>();

            PageSize = PAGESIZE;

            SelectedModal = null;
            Filter = FilterDisciplines;
        }

        public async Task LoadContentAsync()
        {
            if (IsBlocked)
                return;

            (ErrorMessage, Holding) =
                await _apiService.GetAsync<HoldingInfo>("Holding",
                $"getLastHolding", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            TimeZoneInfo fleTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            DateTime kyivDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, fleTimeZone);
            var currentDate = DateOnly.FromDateTime(kyivDateTime);

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

            foreach (var faculty in faculties)
                Faculties.Add(faculty);

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        private async Task LoadTotalPagesAsync()
        {
            (ErrorMessage, var totalSize) =
                await _apiService.GetAsync<int>("Discipline",
                $"getCountForStudent?eduLevel={_userStore.StudentInfo.Group.EduLevel}&holding={Holding.EduYear}" +
                $"{CourseFilter}{CatalogFilter}{SemesterFilter}{FacultyFilter}",
                _userStore.AccessToken);

            if (HasErrorMessage)
                return;

            TotalPages = (int)Math.Ceiling((double)totalSize / PageSize);
            CurrentPage = 0;
        }

        private async Task LoadDisciplinesAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var disciplines) =
                await _apiService.GetAsync<ObservableCollection<DisciplineInfoForStudent>>("Discipline",
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

        partial void OnSelectedCatalogChanged(CatalogTypeInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        partial void OnSelectedFacultyChanged(FacultyInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        partial void OnSelectedSemesterChanged(SemesterInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(LoadTotalPagesAsync);

            if (HasErrorMessage)
                return;

            await LoadDisciplinesAsync(1);
        }

        [RelayCommand]
        private void CloseModal()
        {
            SelectedModal = null;
        }

        [RelayCommand(CanExecute = nameof(IsNextPageEnabled))]
        private async Task NextPage()
        {
            await LoadDisciplinesAsync(CurrentPage + 1);
        }

        [RelayCommand(CanExecute = nameof(IsPreviousPageEnabled))]
        private async Task PreviousPage()
        {
            await LoadDisciplinesAsync(CurrentPage - 1);
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

            if (HasErrorMessage)
                return;

            SelectedModal = new DisciplinePreviewViewModel(CloseModalCommand, discipline);
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private bool FilterDisciplines(object discipline, string filter)
        {
            if (discipline is not DisciplineInfoForStudent disciplineInfo)
                return false;

            return disciplineInfo.DisciplineName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                disciplineInfo.DisciplineCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
