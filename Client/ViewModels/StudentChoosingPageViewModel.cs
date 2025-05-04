using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.CustomControlsViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels
{
    public partial class StudentChoosingPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private const int FALLSEMESTER = 1;
        private const int SPRINGSEMESTER = 2;

        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messageService;

        private readonly int _nonparSemesterCount;
        private readonly int _parSemesterCount;

        public HoldingInfo Holding { get; set; }

        public List<DisciplineComboBoxViewModel> OddSemesterChoices { get; init; }
        public List<DisciplineComboBoxViewModel> EvenSemesterChoices { get; init; }

        [ObservableProperty]
        private bool _isBlocked;

        [ObservableProperty]
        private string? _blockedMessage = default!;

        [ObservableProperty]
        private bool _isHolding;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public StudentChoosingPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messageService = messageService;

            if (_userStore.StudentInfo is null)
                throw new Exception("Доступ обмежено");

            if (_userStore.StudentInfo.Group.Course == _userStore.StudentInfo.Group.DurationOfStudy ||
                _userStore.StudentInfo.Group.Course == 0)
            {
                _isBlocked = true;
                _blockedMessage = "Для вашої групи наразі не запланований вибір дисциплін";
                return;
            }

            _nonparSemesterCount = _userStore.StudentInfo.Group.Nonparsemester;
            _parSemesterCount = _userStore.StudentInfo.Group.Parsemester;

            OddSemesterChoices = new List<DisciplineComboBoxViewModel>(_nonparSemesterCount);
            EvenSemesterChoices = new List<DisciplineComboBoxViewModel>(_parSemesterCount);
        }

        public async Task LoadContentAsync()
        {
            if (IsBlocked)
                return;

            (ErrorMessage, Holding) =
                    await _apiService.GetAsync<HoldingInfo>
                    ("Holding", $"getLastHolding", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            TimeZoneInfo fleTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            DateTime kyivDateTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, fleTimeZone);
            var currentDate = DateOnly.FromDateTime(kyivDateTime);

            if (Holding.EduYear == _userStore.StudentInfo.Group.AdmissionYear && !_userStore.StudentInfo.Group.HasEnterChoise)
            {
                IsBlocked = true;
                BlockedMessage = "Для вашої групи наразі не запланований вибір дисциплін";
                return;
            }

            if (Holding.StartDate > currentDate || currentDate > Holding.EndDate)
            {
                IsHolding = false;
                return;
            }

            IsHolding = true;

            (ErrorMessage, var madeRecords) =
                    await _apiService.GetAsync<List<RecordShortInfo>>
                    ("Record", $"getWithDisciplineShortInfo?year={Holding.EduYear}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            var isAllAccepted = madeRecords is not null && madeRecords
                .All(record => record.Approved == 1) && madeRecords.Count == (_nonparSemesterCount + _parSemesterCount);

            if (isAllAccepted)
            {
                IsBlocked = true;
                BlockedMessage = "Усі ваші вибори було підтверджено, ви можете переглянути їх на відповідній сторінці";
                return;
            }

            var groupedRecords = new Dictionary<int, List<RecordShortInfo>>()
            {
                { FALLSEMESTER, new List<RecordShortInfo>(_nonparSemesterCount) },
                { SPRINGSEMESTER, new List<RecordShortInfo>(_parSemesterCount) }
            };

            foreach (var record in madeRecords ?? Enumerable.Empty<RecordShortInfo>())
                groupedRecords[record.ChosenSemester].Add(record);

            byte searchCourse = (byte)(_userStore.StudentInfo.Group.Course +
            ((_userStore.StudentInfo.Group.AdmissionYear == Holding.EduYear
            && _userStore.StudentInfo.Group.HasEnterChoise) ? 0 : 1));

            searchCourse += _userStore.StudentInfo.Group.ChoiceDifference;

            for (int i = 0; i < _userStore.StudentInfo.Group.Nonparsemester; i++)
                OddSemesterChoices.Add(new DisciplineComboBoxViewModel(_apiService, _userStore.AccessToken, Holding.EduYear,
                    searchCourse, 1, _userStore.StudentInfo.Group.EduLevel, $"Осінній семестр — вибір {i + 1}",
                    groupedRecords[FALLSEMESTER].ElementAtOrDefault(i)));

            for (int i = 0; i < _userStore.StudentInfo.Group.Parsemester; i++)
                EvenSemesterChoices.Add(new DisciplineComboBoxViewModel(_apiService, _userStore.AccessToken, Holding.EduYear,
                    searchCourse, 2, _userStore.StudentInfo.Group.EduLevel, $"Весняний семестр — вибір {i + 1}",
                    groupedRecords[SPRINGSEMESTER].ElementAtOrDefault(i)));
        }

        [RelayCommand(CanExecute = nameof(IsHolding))]
        private async Task Submit()
        {
            int skipCount = 0;

            await ExecuteWithWaiting(async () =>
            {
                ValidateAll();

                if (HasErrorMessage)
                    return;

                skipCount = await Submit(OddSemesterChoices);

                if (HasErrorMessage)
                    return;

                skipCount += await Submit(EvenSemesterChoices);
            });

            if (skipCount == OddSemesterChoices.Count + EvenSemesterChoices.Count)
                _messageService.ShowInfoMessage("Змін не виявлено", "Інформуємо");
        }

        private async Task<int> Submit(List<DisciplineComboBoxViewModel> semesterChoices)
        {
            int skipCount = 0;

            foreach (var semesterChoice in semesterChoices)
            {
                semesterChoice.FaIcon = "None";

                if (semesterChoice.IsDisciplineChanged() == false ||
                    semesterChoices.Any(choice => choice.OldDisciplineId == semesterChoice.SelectedDiscipline.DisciplineId))
                {
                    skipCount++;
                    continue;
                }

                var isSuccess = await semesterChoice.SubmitAsync();

                if (!isSuccess)
                {
                    ErrorMessage = "Не всі записи було зареєстрвоано";
                    return -1;
                }
            }

            return skipCount;
        }

        private void ValidateAll()
        {
            ErrorMessage = string.Empty;

            foreach (var semesterChoice in OddSemesterChoices)
                semesterChoice.ErrorMessage = string.Empty;
            foreach (var semesterChoice in EvenSemesterChoices)
                semesterChoice.ErrorMessage = string.Empty;

            bool hasError = ValidateOnEmpty(OddSemesterChoices) ||
                ValidateOnEmpty(EvenSemesterChoices);

            if (hasError)
                return;

            ValidateOnRepeat();
        }

        private bool ValidateOnEmpty(in List<DisciplineComboBoxViewModel> semesterChoices)
        {
            var emptyChoise = semesterChoices
                .FirstOrDefault(semesterChoice => semesterChoice.SelectedDiscipline is null);

            if (emptyChoise is not null)
            {
                emptyChoise.ErrorMessage = "Не обрано дисципліну";
                ErrorMessage = "Виберіть дисципліни для всіх семестрів";
                return true;
            }

            return false;
        }

        private bool ValidateOnRepeat()
        {
            var uniqueChoices = new Dictionary<uint, byte>();

            bool CheckForDuplicateDisciplines(in List<DisciplineComboBoxViewModel> semesterChoices, byte semester)
            {
                foreach (var semesterChoice in semesterChoices)
                {
                    var disciplineId = semesterChoice.SelectedDiscipline.DisciplineId;

                    if (uniqueChoices.TryGetValue(disciplineId, out byte semesterValue) &&
                    (semesterValue == semester || !semesterChoice.SelectedDiscipline.IsYearLong))
                    {
                        ErrorMessage = "Вибрані дисципліни повторюються";
                        semesterChoice.ErrorMessage = "Дисципліна вже обрана";
                        return true;
                    }

                    uniqueChoices[disciplineId] = semester;
                }

                return false;
            }

            return CheckForDuplicateDisciplines(OddSemesterChoices, FALLSEMESTER) ||
                CheckForDuplicateDisciplines(EvenSemesterChoices, SPRINGSEMESTER);
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
