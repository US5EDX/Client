using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class GroupRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private CancellationTokenSource _cts;

        private readonly UserStore _userStore;
        private readonly ApiService _apiService;

        private readonly ObservableCollection<WorkerShortInfo> _workers;

        public IEnumerable<WorkerShortInfo> Workers => _workers;

        public List<SpecialtyInfo> Specialties { get; init; }
        public List<EduLevelInfo> EduLevels { get; init; }

        [ObservableProperty]
        private string _header;

        private readonly uint _groupId;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 30)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private string _groupCode;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private SpecialtyInfo? _specialty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private EduLevelInfo? _eduLevel;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 4)]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateDurationOfStudy))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private byte? _durationOfStudy;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateAdmissionYear))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private DateTime _admissionYear;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 5)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private byte? _nonparsemester;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(1, 5)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private byte? _parsemester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private bool _hasEnterChoise;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Range(0, 2)]
        [CustomValidation(typeof(GroupRegistryViewModel),
            nameof(ValidateChoiceDifference))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private byte _choiceDifference;

        [ObservableProperty]
        private bool _isShortened;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        private WorkerShortInfo? _worker;

        [ObservableProperty]
        private string _workerFullName;

        [ObservableProperty]
        private bool _isLoading;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !HasErrors &&
            EduLevel is not null &&
            DurationOfStudy is not null &&
            Specialty is not null &&
            Nonparsemester is not null &&
            Parsemester is not null;

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

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

        partial void OnHasEnterChoiseChanged(bool value)
        {
            ValidateProperty(DurationOfStudy, nameof(DurationOfStudy));
        }

        partial void OnIsShortenedChanged(bool value)
        {
            if (!value)
                ChoiceDifference = 0;
        }

        public GroupRegistryViewModel(UserStore userStore, ApiService apiService, List<SpecialtyInfo> specialtiesInfo,
            IRelayCommand closeCommand, GroupFullInfo? groupInfo = null)
        {
            _userStore = userStore;
            _apiService = apiService;

            CloseCommand = closeCommand;
            SubmitCommand = groupInfo is null ? AddGroupCommand : UpdateGroupCommand;

            Header = groupInfo is null ? "Додати групу" : "Редагувати групу";

            Specialties = specialtiesInfo;

            EduLevels = new List<EduLevelInfo>()
            {
                new EduLevelInfo() { EduLevelId = 1, LevelName = "Бакалавр" },
                new EduLevelInfo() { EduLevelId = 2, LevelName = "Магістр" },
                new EduLevelInfo() { EduLevelId = 3, LevelName = "PHD" },
            };

            _workers = new ObservableCollection<WorkerShortInfo>();

            _groupId = groupInfo?.GroupId ?? 0;
            GroupCode = groupInfo?.GroupCode;
            Specialty = Specialties.FirstOrDefault(sp => sp.SpecialtyId == groupInfo?.Specialty.SpecialtyId);
            EduLevel = EduLevels.FirstOrDefault(level => level.EduLevelId == groupInfo?.EduLevel);
            DurationOfStudy = groupInfo?.DurationOfStudy;
            AdmissionYear = new DateTime(groupInfo?.AdmissionYear ?? DateTime.Now.Year, 1, 1);
            Nonparsemester = groupInfo?.Nonparsemester;
            Parsemester = groupInfo?.Parsemester;
            HasEnterChoise = groupInfo?.HasEnterChoise ?? false;
            IsShortened = groupInfo?.ChoiceDifference != 0;
            ChoiceDifference = groupInfo?.ChoiceDifference ?? 0;

            if (groupInfo?.CuratorInfo is not null)
            {
                _workers.Add(groupInfo.CuratorInfo);
                Worker = _workers[0];
                WorkerFullName = Worker.FullName;
                return;
            }

            _workers.Add(new WorkerShortInfo { FullName = "Без куратора" });
        }

        partial void OnWorkerFullNameChanged(string value)
        {
            if (Worker is not null && Worker.FullName == value)
                return;

            if (value.Length < 3)
            {
                _cts?.Cancel();
                _workers.Clear();
                _workers.Add(new WorkerShortInfo { FullName = "Без куратора" });
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
                    _workers.Clear();
                    _workers.Add(new WorkerShortInfo { FullName = "Пошук..." });
                    IsLoading = true;
                });

                var result = await LoadWorkersFromDatabase(value);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    _workers.Clear();
                    foreach (var item in result)
                        _workers.Add(item);
                    IsLoading = false;
                });
            }, token);
        }

        private async Task<IEnumerable<WorkerShortInfo>> LoadWorkersFromDatabase(string searchFilter)
        {
            (ErrorMessage, var workers) =
                await _apiService.GetAsync<ObservableCollection<WorkerShortInfo>>("Worker",
                $"getByFacultyAndFullName?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}" +
                $"&fullName={searchFilter}", _userStore.AccessToken);

            return workers ?? Enumerable.Empty<WorkerShortInfo>();
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddGroup()
        {
            ValidateAllProperties();

            await ExecuteWithWaiting(async () =>
            {
                var newGroup = InithializeInstance();

                (ErrorMessage, var addedGroup) =
                    await _apiService.PostAsync<GroupFullInfo>("Group", "addGroup", newGroup, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedGroup);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateGroup()
        {
            ValidateAllProperties();

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

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
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
