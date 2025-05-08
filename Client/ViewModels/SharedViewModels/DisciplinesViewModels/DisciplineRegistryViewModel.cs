using Client.Models;
using Client.Parsers;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    public partial class DisciplineRegistryViewModel : ViewModelBaseWithValidation
    {
        private readonly DisciplineReaderService _pdfReaderService;
        private readonly IMessageService _messageService;

        private byte _courseMask;
        private bool _isCourseBoolUpdating;

        private readonly uint _disciplineId;
        private readonly uint _facultyId;
        private readonly int _subscribersCount;
        private readonly bool _isOpen;

        public List<CatalogTypeInfo> CatalogTypes { get; init; }
        public List<SpecialtyInfo> Specialties { get; init; }
        public List<EduLevelInfo> EduLevels { get; init; }
        public List<SemesterInfo> Semesters { get; init; }
        public List<short> Holdings { get; init; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 50)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string? _disciplineCode;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 200)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string? _disciplineName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private CatalogTypeInfo? _catalogType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private SpecialtyInfo? _specialty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private EduLevelInfo? _eduLevel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyPropertyChangedFor(nameof(IsBoth))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private SemesterInfo? _semester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private bool _isCourse1Available;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private bool _isCourse2Available;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private bool _isCourse3Available;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private bool _isCourse4Available;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 500)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string? _prerequisites;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 1000)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string? _interest;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 250)]
        [CustomValidation(typeof(DisciplineRegistryViewModel),
            nameof(ValidateMaxCountBiggerMinCount))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private int? _maxCount;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 250)]
        [CustomValidation(typeof(DisciplineRegistryViewModel),
            nameof(ValidateMaxCountBiggerMinCount))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private int? _minCount;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 500)]
        [CustomValidation(typeof(DisciplineRegistryViewModel), nameof(ValidateUrl))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string? _url;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private short? _holding;

        [ObservableProperty]
        private bool _isYearLong;

        public bool IsBoth => Semester?.SemesterId == 0;

        public override bool CanSubmit => !HasErrors && _courseMask != 0 &&
            CatalogType is not null && Specialty is not null &&
            EduLevel is not null && Semester is not null;

        partial void OnMinCountChanged(int? value) => ValidateProperty(MaxCount, nameof(MaxCount));

        public static ValidationResult ValidateMaxCountBiggerMinCount(string name, ValidationContext context)
        {
            DisciplineRegistryViewModel viewModel = (DisciplineRegistryViewModel)context.ObjectInstance;

            if (viewModel.MaxCount == 0 || viewModel.MaxCount >= viewModel.MinCount)
                return ValidationResult.Success;

            return new("Мінімальна кількість студентів не може бути більше за максимальну");
        }

        public static ValidationResult ValidateUrl(string name, ValidationContext context)
        {
            DisciplineRegistryViewModel viewModel = (DisciplineRegistryViewModel)context.ObjectInstance;

            if (Uri.TryCreate(viewModel.Url, UriKind.Absolute, out var outUri)
                    && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                return ValidationResult.Success;

            return new("Введене значення не є посиланням");
        }

        public DisciplineRegistryViewModel(UserStore userStore, ApiService apiService, DisciplineReaderService pdfReaderService,
            IMessageService messageService, List<SpecialtyInfo> specialtiesInfo, short holding,
            IRelayCommand closeCommand, DisciplineFullInfo? discipline = null) :
            base(apiService, userStore, closeCommand)
        {
            _pdfReaderService = pdfReaderService;
            _messageService = messageService;

            IsAddMode = discipline is null;

            SubmitCommand = IsAddMode ? AddCommand : UpdateCommand;

            Header = IsAddMode ? "Додати дисципліну" : "Редагувати дисципліну";

            CatalogTypes =
            [
                new CatalogTypeInfo() { CatalogType = 1, CatalogName = "УВК"},
                new CatalogTypeInfo() { CatalogType = 2, CatalogName = "ФВК"}
            ];

            Specialties = [.. specialtiesInfo.Prepend(new SpecialtyInfo() { SpecialtyId = 0, SpecialtyName = "Не вказано" })];

            EduLevels =
            [
                new EduLevelInfo() { EduLevelId = 1, LevelName = "Бакалавр" },
                new EduLevelInfo() { EduLevelId = 2, LevelName = "Магістр" },
                new EduLevelInfo() { EduLevelId = 3, LevelName = "PHD" },
            ];

            Semesters =
            [
                new SemesterInfo() { SemesterId = 0, SemesterName = "Обидва"},
                new SemesterInfo() { SemesterId = 1, SemesterName = "Непарний"},
                new SemesterInfo() { SemesterId = 2, SemesterName = "Парний"}
            ];

            Holdings = [discipline?.Holding ?? holding];

            _disciplineId = discipline?.DisciplineId ?? 0;
            _facultyId = discipline?.Faculty.FacultyId ?? _userStore.WorkerInfo.Faculty.FacultyId;
            _isOpen = discipline?.IsOpen ?? true;

            InithializeProperties(discipline, holding);
        }

        partial void OnSemesterChanged(SemesterInfo? value)
        {
            if (value?.SemesterId != 0)
                IsYearLong = false;
        }

        partial void OnIsCourse1AvailableChanged(bool value) => UpdateCourseMask(value, 0);

        partial void OnIsCourse2AvailableChanged(bool value) => UpdateCourseMask(value, 1);

        partial void OnIsCourse3AvailableChanged(bool value) => UpdateCourseMask(value, 2);

        partial void OnIsCourse4AvailableChanged(bool value) => UpdateCourseMask(value, 3);

        protected override async Task Add()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var newDiscipline = InitializeInstance();

                (ErrorMessage, var addedDiscipline) =
                    await _apiService.PostAsync<DisciplineFullInfo>("Discipline", "addDiscipline",
                    newDiscipline, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedDiscipline);
            });
        }

        protected override async Task Update()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var changedDiscipline = InitializeInstance();

                (ErrorMessage, var updatedDiscipline) =
                    await _apiService.PutAsync<DisciplineFullInfo>("Discipline", "updateDiscipline",
                    changedDiscipline, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedDiscipline);
            });
        }

        [RelayCommand]
        private void LoadFromFile()
        {
            var path = _messageService.ShowOpenFileDialog("Оберіть документ",
                "Doc files|*.docx;*.doc;*.rtf");

            if (path is null) return;

            List<string> data = null!;

            try
            {
                data = _pdfReaderService.ReadDisciplineDocx(path);
            }
            catch
            {
                _messageService.ShowInfoMessage("Не вдалося прочитати документ", "Помилка");
                return;
            }

            if (data is null || data.Count == 0) return;

            var codeAndName = data.ElementAtOrDefault(0);
            var code = codeAndName?.Split(' ')[0];

            if (code is not null)
                code = code.Contains('_') ? codeAndName?.Split('_')[0] : code;

            DisciplineCode = code?.Trim() ?? string.Empty;
            DisciplineName = codeAndName?.Substring(DisciplineCode.Length + 1).Trim();
            CatalogType = DisciplineCode.Contains('у') ? CatalogTypes[0] : CatalogTypes[1];

            var eduLevel = data.ElementAtOrDefault(1);
            var eduLevelId = eduLevel is null ? 1 : eduLevel.Contains("перший", StringComparison.CurrentCultureIgnoreCase) ? 1 :
                (eduLevel.Contains("другий", StringComparison.CurrentCultureIgnoreCase) ? 2 : 3);

            EduLevel = EduLevels.First(level => level.EduLevelId == eduLevelId);

            _courseMask = CourseParser.ParseCourseString(data.ElementAtOrDefault(2)?.Trim());
            UpdateCourseBooleans();

            var semesterId = SemesterParser.ParseSemesterString(data.ElementAtOrDefault(2)?.Trim());
            Semester = Semesters[semesterId];

            Prerequisites = data.ElementAtOrDefault(3)?.Trim();
            Interest = data.ElementAtOrDefault(4)?.Trim();

            var isSuccess = int.TryParse(data.ElementAtOrDefault(5), out int maxCount);
            MaxCount = isSuccess ? maxCount : 0;

            isSuccess = int.TryParse(data.ElementAtOrDefault(6), out int minCount);
            MinCount = isSuccess ? minCount : 0;
        }

        private void UpdateCourseBooleans()
        {
            _isCourseBoolUpdating = true;
            IsCourse1Available = (_courseMask & (1 << 0)) != 0;
            IsCourse2Available = (_courseMask & (1 << 1)) != 0;
            IsCourse3Available = (_courseMask & (1 << 2)) != 0;
            IsCourse4Available = (_courseMask & (1 << 3)) != 0;
            _isCourseBoolUpdating = false;
        }

        private void UpdateCourseMask(bool isChecked, byte shift)
        {
            if (_isCourseBoolUpdating) return;

            if (isChecked)
                _courseMask |= (byte)(1 << shift);
            else
                _courseMask ^= (byte)(1 << shift);
        }

        private void OnSubmitAccepted(DisciplineFullInfo disciplineInfo)
        {
            WeakReferenceMessenger.Default.Send(new DisciplineUpdatedMessage(disciplineInfo));
            CloseCommand.Execute(null);
        }

        private void InithializeProperties(DisciplineFullInfo? discipline, short holding)
        {
            _disciplineCode = discipline?.DisciplineCode;
            _disciplineName = discipline?.DisciplineName;
            _catalogType = CatalogTypes.FirstOrDefault(c => c.CatalogType == discipline?.CatalogType);
            _specialty = Specialties.FirstOrDefault(s => s.SpecialtyId == discipline?.Specialty?.SpecialtyId) ?? Specialties[0];
            _eduLevel = EduLevels.FirstOrDefault(e => e.EduLevelId == discipline?.EduLevel);
            _courseMask = CourseParser.ParseFormatedCourseString(discipline?.Course);
            _semester = Semesters.FirstOrDefault(s => s.SemesterId == discipline?.Semester);
            _prerequisites = discipline?.Prerequisites;
            _interest = discipline?.Interest;
            _maxCount = discipline?.MaxCount ?? 0;
            _minCount = discipline?.MinCount ?? 0;
            _url = discipline?.Url;
            _holding = discipline?.Holding ?? holding;
            _isYearLong = discipline?.IsYearLong ?? false;

            UpdateCourseBooleans();
        }

        private DisciplineRegistryInfo InitializeInstance()
        {
            return new DisciplineRegistryInfo
            {
                DisciplineId = _disciplineId,
                DisciplineCode = DisciplineCode,
                CatalogType = CatalogType.CatalogType,
                FacultyId = _facultyId,
                SpecialtyId = (Specialty is null || Specialty.SpecialtyId == 0) ? null : Specialty.SpecialtyId,
                DisciplineName = DisciplineName,
                EduLevel = EduLevel.EduLevelId,
                Course = _courseMask,
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
