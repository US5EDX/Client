using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class DisciplineRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;
        private readonly DisciplineReaderService _pdfReaderService;
        private readonly IMessageService _messageService;

        public List<CatalogTypeInfo> CatalogTypes { get; init; }
        public List<SpecialtyInfo> Specialties { get; init; }
        public List<EduLevelInfo> EduLevels { get; init; }
        public List<SemesterInfo> Semesters { get; init; }
        public List<short> Holdings { get; init; }

        [ObservableProperty]
        private string _header;

        private readonly uint _disciplineId;
        private readonly FacultyInfo _faculty;
        private readonly int _subscribersCount;
        private readonly bool _isOpen;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 50)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _disciplineCode;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 200)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _disciplineName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private CatalogTypeInfo? _catalogType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private SpecialtyInfo? _specialty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private EduLevelInfo? _eduLevel;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 250)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _course;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyPropertyChangedFor(nameof(IsBoth))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private SemesterInfo? _semester;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 500)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _prerequisites;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 1000)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _interest;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 250)]
        [CustomValidation(typeof(DisciplineRegistryViewModel),
            nameof(ValidateMaxCountBiggerMinCount))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private int? _maxCount;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 250)]
        [CustomValidation(typeof(DisciplineRegistryViewModel),
            nameof(ValidateMaxCountBiggerMinCount))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private int? _minCount;

        partial void OnMinCountChanged(int? value)
        {
            ValidateProperty(MaxCount, nameof(MaxCount));
        }

        public static ValidationResult ValidateMaxCountBiggerMinCount(string name, ValidationContext context)
        {
            DisciplineRegistryViewModel viewModel = (DisciplineRegistryViewModel)context.ObjectInstance;

            if (viewModel.MaxCount == 0 || viewModel.MaxCount >= viewModel.MinCount)
                return ValidationResult.Success;

            return new("Мінімальна кількість студентів не може бути більше за максимальну");
        }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 500)]
        [CustomValidation(typeof(DisciplineRegistryViewModel), nameof(ValidateUrl))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private string _url;

        public static ValidationResult ValidateUrl(string name, ValidationContext context)
        {
            DisciplineRegistryViewModel viewModel = (DisciplineRegistryViewModel)context.ObjectInstance;

            if (Uri.TryCreate(viewModel.Url, UriKind.Absolute, out var outUri)
                    && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new("Введене значення не є посиланням");
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddDisciplineCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateDisciplineCommand))]
        private short? _holding;

        [ObservableProperty]
        private bool _isYearLong;

        public bool IsBoth => Semester?.SemesterId == 0;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !HasErrors;

        public bool IsAddMode { get; init; }

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public DisciplineRegistryViewModel(UserStore userStore, ApiService apiService, DisciplineReaderService pdfReaderService,
            IMessageService messageService, List<SpecialtyInfo> specialtiesInfo, short holding,
            IRelayCommand closeCommand, DisciplineFullInfo? discipline = null)
        {
            _userStore = userStore;
            _apiService = apiService;
            _pdfReaderService = pdfReaderService;
            _messageService = messageService;

            CloseCommand = closeCommand;
            SubmitCommand = discipline is null ? AddDisciplineCommand : UpdateDisciplineCommand;

            Header = discipline is null ? "Додати дисципліну" : "Редагувати дисципліну";
            IsAddMode = discipline is null;

            CatalogTypes = new List<CatalogTypeInfo>()
            {
                new CatalogTypeInfo() { CatalogType = 1, CatalogName = "УВК"},
                new CatalogTypeInfo() { CatalogType = 2, CatalogName = "ФВК"}
            };

            Specialties = [.. specialtiesInfo.Prepend(new SpecialtyInfo() { SpecialtyId = 0, SpecialtyName = "Не вказано" })];

            EduLevels = new List<EduLevelInfo>()
            {
                new EduLevelInfo() { EduLevelId = 1, LevelName = "Бакалавр" },
                new EduLevelInfo() { EduLevelId = 2, LevelName = "Магістр" },
                new EduLevelInfo() { EduLevelId = 3, LevelName = "PHD" },
            };

            Semesters = new List<SemesterInfo>()
            {
                new SemesterInfo() { SemesterId = 0, SemesterName = "Обидва"},
                new SemesterInfo() { SemesterId = 1, SemesterName = "Непарний"},
                new SemesterInfo() { SemesterId = 2, SemesterName = "Парний"}
            };

            Holdings = new List<short>() { discipline?.Holding ?? holding };

            _disciplineId = discipline?.DisciplineId ?? 0;
            _faculty = discipline?.Faculty ?? _userStore.WorkerInfo.Faculty;
            _isOpen = discipline?.IsOpen ?? true;

            _disciplineCode = discipline?.DisciplineCode ?? null;
            _disciplineName = discipline?.DisciplineName ?? null;
            _catalogType = CatalogTypes.FirstOrDefault(c => c.CatalogType == discipline?.CatalogType);
            _specialty = Specialties.FirstOrDefault(s => s.SpecialtyId == discipline?.Specialty?.SpecialtyId) ?? Specialties[0];
            _eduLevel = EduLevels.FirstOrDefault(e => e.EduLevelId == discipline?.EduLevel);
            _course = discipline?.Course ?? null;
            _semester = Semesters.FirstOrDefault(s => s.SemesterId == discipline?.Semester);
            _prerequisites = discipline?.Prerequisites ?? null;
            _interest = discipline?.Interest ?? null;
            _maxCount = discipline?.MaxCount ?? 0;
            _minCount = discipline?.MinCount ?? 0;
            _url = discipline?.Url ?? null;
            _holding = discipline?.Holding ?? holding;
            _isYearLong = discipline?.IsYearLong ?? false;
        }

        partial void OnSemesterChanged(SemesterInfo? value)
        {
            if (value?.SemesterId != 0)
                IsYearLong = false;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddDiscipline()
        {
            ValidateAllProperties();

            if (HasErrors)
                return;

            await ExecuteWithWaiting(async () =>
            {
                var newDiscipline = InitializeDiscipline();

                (ErrorMessage, var addedDiscipline) =
                    await _apiService.PostAsync<DisciplineFullInfo>("Discipline", "addDiscipline", newDiscipline, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(newDiscipline);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateDiscipline()
        {
            ValidateAllProperties();

            if (HasErrors)
                return;

            await ExecuteWithWaiting(async () =>
            {
                var updatedDiscipline = InitializeDiscipline();

                (ErrorMessage, _) =
                    await _apiService.PutAsync<object>("Discipline", "updateDiscipline",
                    updatedDiscipline, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedDiscipline);
            });
        }

        [RelayCommand]
        private void LoadFromFile()
        {
            var path = _messageService.ShowOpenFileDialog("Оберіть документ",
                "Doc files|*.docx;*.doc;*.rtf");

            if (path is null)
                return;

            List<string> data;

            try
            {
                data = _pdfReaderService.ReadDisciplineDocx(path);
            }
            catch
            {
                _messageService.ShowInfoMessage("Не вдалося прочитати документ", "Помилка");
                return;
            }

            if (data is null)
                return;

            var codeAndName = data.ElementAtOrDefault(0);

            var code = codeAndName?.Split(' ')[0];
            if (code is not null)
                code = code.Contains('_') ? codeAndName?.Split('_')[0] : code;

            DisciplineCode = code.Trim();
            DisciplineName = codeAndName?.Substring(code.Length + 1).Trim();

            if (DisciplineCode is not null)
                CatalogType = code.Contains('у') ? CatalogTypes[0] : CatalogTypes[1];

            var eduLevel = data.ElementAtOrDefault(1);
            var eduLevelId = eduLevel.ToLower().Contains("перший") ? 1 : (eduLevel.ToLower().Contains("другий") ? 2 : 3);

            EduLevel = EduLevels.First(level => level.EduLevelId == eduLevelId);
            Course = data.ElementAtOrDefault(2).Trim();
            Prerequisites = data.ElementAtOrDefault(3).Trim();
            Interest = data.ElementAtOrDefault(4).Trim();

            var isSuccess = int.TryParse(data.ElementAtOrDefault(5), out int maxCount);
            MaxCount = isSuccess ? maxCount : 0;

            isSuccess = int.TryParse(data.ElementAtOrDefault(6), out int minCount);
            MinCount = isSuccess ? minCount : 0;
        }

        private void OnSubmitAccepted(DisciplineFullInfo disciplineInfo)
        {
            WeakReferenceMessenger.Default.Send(new DisciplineUpdatedMessage(disciplineInfo));
            CloseCommand.Execute(null);
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private DisciplineFullInfo InitializeDiscipline()
        {
            return new DisciplineFullInfo
            {
                DisciplineId = _disciplineId,
                DisciplineCode = DisciplineCode,
                CatalogType = CatalogType.CatalogType,
                Faculty = _faculty,
                Specialty = (Specialty is null || Specialty.SpecialtyId == 0) ? null : Specialty,
                DisciplineName = DisciplineName,
                EduLevel = EduLevel.EduLevelId,
                Course = Course,
                Semester = Semester.SemesterId,
                Prerequisites = Prerequisites,
                Interest = Interest,
                MaxCount = MaxCount ?? 0,
                MinCount = MinCount ?? 0,
                Url = Url,
                Holding = Holding.Value,
                IsYearLong = IsYearLong,
                IsOpen = _isOpen
            };
        }
    }
}
