using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class DisciplinesPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private const int PAGESIZE = 30;

        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly DisciplineMainInfoStore _disciplineStore;
        private readonly IMessageService _messageService;
        private readonly DisciplineReaderService _pdfReaderService;

        private readonly List<SpecialtyInfo> _specialtiesInfo;

        private readonly List<CatalogTypeInfo> _catalogTypes;
        private readonly List<short> _holdings;
        private readonly ObservableCollection<DisciplineWithSubCounts> _disciplines;

        public IEnumerable<DisciplineWithSubCounts> Disciplines => _disciplines;
        public IEnumerable<short> Holdings => _holdings;
        public IEnumerable<CatalogTypeInfo> CatalogTypes => _catalogTypes;

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

        public bool IsNotLocked => !IsWaiting;

        private string CatalogFilter => SelectedCatalog?.CatalogType == 0 ? string.Empty : $"&catalogFilter={SelectedCatalog.CatalogType}";

        private string HoldingFilter => SelectedHolding is null ? string.Empty : $"&holdingFilter={SelectedHolding.Value}";

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsDisciplineSelected => SelectedDiscipline is not null;

        public bool IsNextPageEnabled => CurrentPage < TotalPages;
        public bool IsPreviousPageEnabled => CurrentPage > 1;

        public Func<object, string, bool> Filter { get; init; }

        public DisciplinesPageViewModel(ApiService apiService, UserStore userStore,
            IMessageService messageService, DisciplineReaderService pdfReaderService, DisciplineMainInfoStore disciplineStore)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messageService = messageService;
            _pdfReaderService = pdfReaderService;

            if (_userStore.Role > 3)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            _catalogTypes = new List<CatalogTypeInfo>()
            {
                new CatalogTypeInfo() { CatalogType = 0, CatalogName = "Обидва"},
                new CatalogTypeInfo() { CatalogType = 1, CatalogName = "УВК"},
                new CatalogTypeInfo() { CatalogType = 2, CatalogName = "ФВК"},
            };

            _selectedCatalog = _catalogTypes[0];

            _holdings = new List<short>();
            _disciplines = new ObservableCollection<DisciplineWithSubCounts>();

            _specialtiesInfo = new List<SpecialtyInfo>();

            WeakReferenceMessenger.Default.Register<DisciplineUpdatedMessage>(this, Receive);

            PageSize = PAGESIZE;

            SelectedModal = null;
            Filter = FilterDisciplines;
            _disciplineStore = disciplineStore;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var holdings) =
                await _apiService.GetAsync<List<short>>("Holding",
                $"getLastFive", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            if (holdings is null || holdings.Count == 0)
                throw new Exception("Не вдалось отримати дані про проведення");

            foreach (var holding in holdings)
                _holdings.Add(holding);

            _selectedHolding = _holdings[0];

            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<List<SpecialtyInfo>>
                ("Specialty", $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _specialtiesInfo.Clear();

            foreach (var specialty in specialties ?? Enumerable.Empty<SpecialtyInfo>())
                _specialtiesInfo.Add(specialty);

            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        private async Task LoadTotalPagesAsync()
        {
            (ErrorMessage, var totalSize) =
                await _apiService.GetAsync<int>("Discipline",
                $"getCount?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{HoldingFilter}{CatalogFilter}", _userStore.AccessToken);

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
                await _apiService.GetAsync<ObservableCollection<DisciplineWithSubCounts>>("Discipline",
                $"getDisciplines?pageNumber={page}&pageSize={PageSize}" +
                $"&facultyId={_userStore.WorkerInfo.Faculty.FacultyId}{HoldingFilter}{CatalogFilter}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _disciplines.Clear();

                    foreach (var discipline in disciplines ?? Enumerable.Empty<DisciplineWithSubCounts>())
                        _disciplines.Add(discipline);

                    if (_disciplines.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        partial void OnSelectedCatalogChanged(CatalogTypeInfo? value)
        {
            UpdateListingAsync().ConfigureAwait(false);
        }

        partial void OnSelectedHoldingChanged(short? value)
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

        [RelayCommand]
        private void OpenAddModal()
        {
            SelectedModal = new DisciplineRegistryViewModel(_userStore, _apiService, _pdfReaderService, _messageService,
                _specialtiesInfo, _holdings[0], CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private void OpenUpdateModal()
        {
            SelectedModal = new DisciplineRegistryViewModel(_userStore, _apiService, _pdfReaderService, _messageService,
                _specialtiesInfo, _holdings[0], CloseModalCommand, SelectedDiscipline);
        }

        public void Receive(object recipient, DisciplineUpdatedMessage message)
        {
            DisciplineFullInfo discipline = message.Value;

            if (IsDisciplineSelected && SelectedDiscipline.DisciplineId == discipline.DisciplineId)
            {
                SelectedDiscipline.DisciplineCode = discipline.DisciplineCode;
                SelectedDiscipline.DisciplineName = discipline.DisciplineName;
                SelectedDiscipline.CatalogType = discipline.CatalogType;
                SelectedDiscipline.Specialty = discipline.Specialty;
                SelectedDiscipline.EduLevel = discipline.EduLevel;
                SelectedDiscipline.Course = discipline.Course;
                SelectedDiscipline.Semester = discipline.Semester;
                SelectedDiscipline.Prerequisites = discipline.Prerequisites;
                SelectedDiscipline.Interest = discipline.Interest;
                SelectedDiscipline.MaxCount = discipline.MaxCount;
                SelectedDiscipline.MinCount = discipline.MinCount;
                SelectedDiscipline.Url = discipline.Url;
                SelectedDiscipline = null;
                return;
            }

            _disciplines.Add(new DisciplineWithSubCounts(discipline));
            SelectedDiscipline = null;
        }

        [RelayCommand(CanExecute = nameof(IsDisciplineSelected))]
        private async Task DeleteDiscipline()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити дисципліну {SelectedDiscipline.DisciplineCode}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Discipline", $"deleteDiscipline/{SelectedDiscipline.DisciplineId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _disciplines.Remove(SelectedDiscipline);
                    SelectedDiscipline = null;
                }
            });
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
        private async Task UpdateStatus()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете оновити статус дисципліни {SelectedDiscipline.DisciplineCode}");

            if (!isOk)
                return;

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
        private void NavigateToFullInfo()
        {
            SelectedModal = new DisciplinePreviewViewModel(CloseModalCommand, SelectedDiscipline);
        }

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
        private async Task OpenPrint()
        {
            if (SelectedHolding is null)
                return;

            SelectedModal = new PrintDisciplinesPageViewModel(_apiService, _userStore,
                _messageService, CloseModalCommand, CatalogTypes.Skip(1), Holdings.Take(3));
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
            if (discipline is not DisciplineFullInfo disciplineInfo)
                return false;

            return disciplineInfo.DisciplineName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                disciplineInfo.DisciplineCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
