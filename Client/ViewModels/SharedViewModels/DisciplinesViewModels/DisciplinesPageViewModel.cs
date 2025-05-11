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
    public partial class DisciplinesPageViewModel : PaginationFrameViewModelBase
    {
        private readonly DisciplineMainInfoStore _disciplineStore;
        private readonly IMessageService _messageService;
        private readonly DisciplineReaderService _pdfReaderService;
        private readonly LecturerInfoStore _lecturerInfoStore;

        private readonly List<SpecialtyInfo> _specialtiesInfo;

        public DisciplineStatusThresholds DisciplineStatusThresholds { get; set; }

        public List<CatalogTypeInfo> CatalogTypes { get; init; }
        public List<short> Holdings { get; init; }
        public List<SemesterInfo> Semesters { get; init; }

        public ObservableCollection<DisciplineWithSubCounts> Disciplines { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDisciplineSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateStatusCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToFullInfoCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToStudentsCommand))]
        private DisciplineWithSubCounts? _selectedDiscipline;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CatalogFilter))]
        private CatalogTypeInfo? _selectedCatalog;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HoldingFilter))]
        private short? _selectedHolding;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SemesterFilter))]
        [NotifyPropertyChangedFor(nameof(IsThresholdsButtonVisible))]
        [NotifyPropertyChangedFor(nameof(IsNonParSemesterVisible))]
        [NotifyPropertyChangedFor(nameof(IsParSemesterVisible))]
        private SemesterInfo? _selectedSemester;

        [ObservableProperty]
        private bool _isThresholdsVisible;

        public bool IsDisciplineSelected => SelectedDiscipline is not null;

        public bool IsAdmin => _userStore.Role == 2;

        public bool IsMainNavigation { get; init; }

        public bool CanOpenPrint => IsAdmin && IsMainNavigation;

        public bool IsThresholdsButtonVisible => SelectedSemester?.SemesterId != 0;
        public bool IsNonParSemesterVisible => SelectedSemester?.SemesterId != 2;
        public bool IsParSemesterVisible => SelectedSemester?.SemesterId != 1;

        private string CatalogFilter => SelectedCatalog?.CatalogType == 0 ? string.Empty :
            $"&catalogFilter={SelectedCatalog.CatalogType}";

        private string HoldingFilter => SelectedHolding is null ? string.Empty : $"&holdingFilter={SelectedHolding.Value}";

        private string SemesterFilter => SelectedSemester?.SemesterId == 0 ? string.Empty :
            $"&semesterFilter={SelectedSemester.SemesterId}";

        private string LecturerFilter => _userStore.Role == 3 ? $"&lecturerFilter={_userStore.UserId}" :
            IsMainNavigation ? string.Empty : $"&lecturerFilter={_lecturerInfoStore.LecturerId}";

        public DisciplinesPageViewModel(ApiService apiService, UserStore userStore,
            IMessageService messageService, DisciplineReaderService pdfReaderService, DisciplineMainInfoStore disciplineStore,
            LecturerInfoStore lecturerInfoStore) :
            base(apiService, userStore)
        {
            if (_userStore.Role > 3)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            _messageService = messageService;
            _pdfReaderService = pdfReaderService;
            _lecturerInfoStore = lecturerInfoStore;

            IsMainNavigation = !_lecturerInfoStore.IsActual;

            CatalogTypes =
            [
                new() { CatalogType = 0, CatalogName = "Обидва"},
                new() { CatalogType = 1, CatalogName = "УВК"},
                new() { CatalogType = 2, CatalogName = "ФВК"},
            ];

            _selectedCatalog = CatalogTypes[0];

            Semesters =
            [
                new() {SemesterId = 0, SemesterName = "Обидва"},
                new() {SemesterId = 1, SemesterName = "Осінній"},
                new() {SemesterId = 2, SemesterName = "Весняний"},
            ];

            _selectedSemester = Semesters[0];

            Holdings = [];
            Disciplines = [];
            _specialtiesInfo = [];

            WeakReferenceMessenger.Default.Register<DisciplineUpdatedMessage>(this, Receive);

            PageSize = 30;

            _disciplineStore = disciplineStore;

            Filter = FilterDisciplines;
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var holdings) =
                await _apiService.GetAsync<List<short>>("Holding",
                $"getLastFive", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            if (holdings is null || holdings.Count == 0)
                throw new Exception("Не вдалось отримати дані про проведення");

            Holdings.AddRange(holdings);
            _selectedHolding = Holdings[0];

            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<List<SpecialtyInfo>>
                ("Specialty", $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _specialtiesInfo.AddRange(specialties ?? Enumerable.Empty<SpecialtyInfo>());

            (ErrorMessage, var thresholds) =
                await _apiService.GetAsync<DisciplineStatusThresholds>
                ("Discipline", $"getTresholds", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            DisciplineStatusThresholds = thresholds;

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _lecturerInfoStore.IsActual = false;
        }

        async partial void OnSelectedCatalogChanged(CatalogTypeInfo? value) => await UpdateListingAsync();

        async partial void OnSelectedHoldingChanged(short? value) => await UpdateListingAsync();

        async partial void OnSelectedSemesterChanged(SemesterInfo? value) => await UpdateListingAsync();

        protected override async Task LoadDataAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var disciplines) =
                await _apiService.GetAsync<ObservableCollection<DisciplineWithSubCounts>>("Discipline",
                $"getDisciplines?pageNumber={page}&pageSize={PageSize}" +
                $"&facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                $"{HoldingFilter}{CatalogFilter}{SemesterFilter}{LecturerFilter}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Disciplines.Clear();

                    foreach (var discipline in disciplines ?? Enumerable.Empty<DisciplineWithSubCounts>())
                        Disciplines.Add(discipline);

                    if (Disciplines.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        private async Task UpdateListingAsync()
        {
            await ExecuteWithWaiting(async () => await LoadTotalPagesAsync("Discipline",
                $"getCount?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                $"{HoldingFilter}{CatalogFilter}{SemesterFilter}{LecturerFilter}"));

            if (HasErrorMessage) return;

            await LoadDataAsync(1);
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new DisciplineRegistryViewModel(_userStore, _apiService,
            _pdfReaderService, _messageService, _specialtiesInfo, Holdings[0], CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private void OpenUpdateModal() => SelectedModal = new DisciplineRegistryViewModel(_userStore, _apiService,
            _pdfReaderService, _messageService, _specialtiesInfo, Holdings[0], CloseModalCommand, SelectedDiscipline);

        public void Receive(object recipient, DisciplineUpdatedMessage message)
        {
            DisciplineFullInfo discipline = message.Value;

            if (IsDisciplineSelected && SelectedDiscipline.DisciplineId == discipline.DisciplineId)
                SelectedDiscipline.UpdateInfo(discipline);
            else
                Disciplines.Add(new DisciplineWithSubCounts(discipline));

            SelectedDiscipline = null;
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private async Task DeleteDiscipline()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити дисципліну {SelectedDiscipline.DisciplineCode}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Discipline", $"deleteDiscipline/{SelectedDiscipline.DisciplineId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Disciplines.Remove(SelectedDiscipline);
                    SelectedDiscipline = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private async Task UpdateStatus()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити статус дисципліни {SelectedDiscipline.DisciplineCode}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>(
                        "Discipline", $"updateDisciplineStatus/{SelectedDiscipline.DisciplineId}", null, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedDiscipline.IsOpen = !SelectedDiscipline.IsOpen;
                    SelectedDiscipline = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private void NavigateToFullInfo() => SelectedModal = new DisciplinePreviewViewModel(CloseModalCommand, SelectedDiscipline);

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private async Task NavigateToStudents()
        {
            _disciplineStore.DisciplineId = SelectedDiscipline.DisciplineId;
            _disciplineStore.DisciplineCode = SelectedDiscipline.DisciplineCode;
            _disciplineStore.DisciplineName = SelectedDiscipline.DisciplineName;
            _disciplineStore.Semester = SelectedDiscipline.Semester;

            var viewModel = new SignedStudentsPageViewModel(_apiService, _userStore, _disciplineStore,
                _messageService, CloseModalCommand);

            try
            {
                await viewModel.LoadContentAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return;
            }

            SelectedModal = viewModel;
        }

        [RelayCommand]
        private void OpenPrint() => SelectedModal = new PrintDisciplinesPageViewModel(_apiService, _userStore,
                _messageService, CloseModalCommand, CatalogTypes.Skip(1), Holdings.Take(3));

        [RelayCommand]
        private void Thresholds() => IsThresholdsVisible = !IsThresholdsVisible;

        private bool FilterDisciplines(object discipline, string filter)
        {
            if (discipline is not DisciplineWithSubCounts disciplineInfo) return false;

            return disciplineInfo.DisciplineName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                disciplineInfo.DisciplineCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
