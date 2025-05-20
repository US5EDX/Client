using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    public partial class GroupRegistryViewModel : ViewModelBaseValidationExtended
    {
        private CancellationTokenSource _cts = null!;

        private readonly uint _groupId;

        public ObservableCollection<WorkerShortInfo> Workers { get; init; }

        public List<SpecialtyInfo> Specialties { get; init; }
        public List<EduLevelInfo> EduLevels { get; init; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 30)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string _groupCode = null!;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private SpecialtyInfo? _specialty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private EduLevelInfo? _eduLevel;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 4)]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateDurationOfStudy))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private byte? _durationOfStudy;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateAdmissionYear))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private DateTime _admissionYear;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 5)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private byte? _nonparsemester;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 5)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private byte? _parsemester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private bool _hasEnterChoise;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 2)]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateChoiceDifference))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private byte _choiceDifference;

        [ObservableProperty]
        private bool _isShortened;

        [ObservableProperty]
        private WorkerShortInfo? _worker;

        [ObservableProperty]
        private string _workerFullName = default!;

        [ObservableProperty]
        private bool _isLoading;

        public override bool CanSubmit => !HasErrors &&
            Specialty is not null &&
            EduLevel is not null &&
            DurationOfStudy is not null &&
            Nonparsemester is not null &&
            Parsemester is not null;

        public static ValidationResult ValidateAdmissionYear(string name, ValidationContext context)
        {
            GroupRegistryViewModel viewModel = (GroupRegistryViewModel)context.ObjectInstance;

            if (viewModel.AdmissionYear.Year > 2019 && viewModel.AdmissionYear.Year < DateTime.UtcNow.Year + 1)
                return ValidationResult.Success;

            return new("Навчальний рік повинен бути у межах 2020 - 2155");
        }

        public static ValidationResult ValidateDurationOfStudy(string name, ValidationContext context)
        {
            GroupRegistryViewModel viewModel = (GroupRegistryViewModel)context.ObjectInstance;

            if (viewModel.DurationOfStudy is not null
                && viewModel.DurationOfStudy == 1 && viewModel.HasEnterChoise == false)
                return new("Довжина навчання не може дорівнювати 1 року, якщо при вступі немає вибору");

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateChoiceDifference(string name, ValidationContext context)
        {
            GroupRegistryViewModel viewModel = (GroupRegistryViewModel)context.ObjectInstance;

            if (viewModel.DurationOfStudy is not null
                && viewModel.DurationOfStudy + viewModel.ChoiceDifference > 4)
                return new("Різниця не може бути такою, що студенти будть обирати на курс > 4");

            return ValidationResult.Success;
        }

        partial void OnHasEnterChoiseChanged(bool value) => ValidateProperty(DurationOfStudy, nameof(DurationOfStudy));

        partial void OnIsShortenedChanged(bool value)
        {
            if (!value)
                ChoiceDifference = 0;
        }

        public GroupRegistryViewModel(UserStore userStore, ApiService apiService, List<SpecialtyInfo> specialtiesInfo,
            IRelayCommand closeCommand, GroupFullInfo? groupInfo = null) :
            base(apiService, userStore, closeCommand)
        {
            if (_userStore.Role != 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            SubmitCommand = groupInfo is null ? AddCommand : UpdateCommand;

            Header = groupInfo is null ? "Додати групу" : "Редагувати групу";

            Specialties = specialtiesInfo;

            EduLevels =
            [
                new EduLevelInfo() { EduLevelId = 1, LevelName = "Бакалавр" },
                new EduLevelInfo() { EduLevelId = 2, LevelName = "Магістр" },
                new EduLevelInfo() { EduLevelId = 3, LevelName = "PHD" },
            ];

            Workers = [];

            _groupId = groupInfo?.GroupId ?? 0;

            InithializeProperties(groupInfo);
        }

        partial void OnWorkerFullNameChanged(string value)
        {
            if (Worker is not null && Worker.FullName == value)
                return;

            if (value.Length < 3)
            {
                _cts?.Cancel();
                Workers.Clear();
                Workers.Add(new WorkerShortInfo { FullName = "Без куратора" });
                Worker = null;
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                await Task.Delay(500);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Workers.Clear();
                    Workers.Add(new WorkerShortInfo { FullName = "Пошук..." });
                    IsLoading = true;
                });

                var result = await LoadWorkersAsync(value);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Workers.Clear();
                    foreach (var item in result)
                        Workers.Add(item);
                    IsLoading = false;
                });
            }, token);
        }

        private async Task<IEnumerable<WorkerShortInfo>> LoadWorkersAsync(string searchFilter)
        {
            (ErrorMessage, var workers) =
                await _apiService.GetAsync<ObservableCollection<WorkerShortInfo>>("Worker",
                $"getByFacultyAndFullName?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                $"&fullName={searchFilter}", _userStore.AccessToken);

            return workers ?? Enumerable.Empty<WorkerShortInfo>();
        }

        protected override async Task Add()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var newGroup = InithializeInstance();

                (ErrorMessage, var addedGroup) =
                    await _apiService.PostAsync<GroupFullInfo>("Group", "addGroup", newGroup, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedGroup);
            });
        }

        protected override async Task Update()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var updatingGroup = InithializeInstance();

                (ErrorMessage, var updatedGroup) =
                    await _apiService.PutAsync<GroupFullInfo>("Group", "updateGroup", updatingGroup, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedGroup);
            });
        }

        private void OnSubmitAccepted(GroupFullInfo groupInfo)
        {
            WeakReferenceMessenger.Default.Send(new GroupUpdatedMessage(groupInfo));
            CloseCommand.Execute(null);
        }

        private void InithializeProperties(GroupFullInfo? groupInfo)
        {
            _groupCode = groupInfo?.GroupCode;
            _specialty = Specialties.FirstOrDefault(sp => sp.SpecialtyId == groupInfo?.Specialty.SpecialtyId);
            _eduLevel = EduLevels.FirstOrDefault(level => level.EduLevelId == groupInfo?.EduLevel);
            _durationOfStudy = groupInfo?.DurationOfStudy;
            _admissionYear = new DateTime(groupInfo?.AdmissionYear ?? DateTime.Now.Year, 1, 1);
            _nonparsemester = groupInfo?.Nonparsemester;
            _parsemester = groupInfo?.Parsemester;
            _hasEnterChoise = groupInfo?.HasEnterChoise ?? false;
            _isShortened = groupInfo is null ? false : groupInfo.ChoiceDifference != 0;
            _choiceDifference = groupInfo?.ChoiceDifference ?? 0;

            if (groupInfo?.CuratorInfo is not null)
            {
                Workers.Add(groupInfo.CuratorInfo);
                _worker = Workers[0];
                _workerFullName = Worker.FullName;
                return;
            }

            Workers.Add(new WorkerShortInfo { FullName = "Без куратора" });
        }

        private GroupRegistryInfo InithializeInstance()
        {
            return new GroupRegistryInfo
            {
                GroupId = _groupId == 0 ? null : _groupId,
                GroupCode = GroupCode,
                SpecialtyId = Specialty.SpecialtyId,
                EduLevel = EduLevel.EduLevelId,
                DurationOfStudy = DurationOfStudy.Value,
                AdmissionYear = (short)AdmissionYear.Year,
                Nonparsemester = Nonparsemester.Value,
                Parsemester = Parsemester.Value,
                HasEnterChoise = HasEnterChoise,
                ChoiceDifference = ChoiceDifference,
                CuratorId = Worker?.WorkerId
            };
        }
    }
}
