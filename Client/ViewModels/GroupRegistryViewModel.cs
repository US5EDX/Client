using Client.Models;
using Client.Services;
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
    public partial class GroupRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;

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
        [Range(0, 12)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private byte? _course;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateGroupCommand))]
        private EduLevelInfo? _eduLevel;

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
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !HasErrors &&
            EduLevel is not null &&
            Specialty is not null &&
            Course is not null &&
            Nonparsemester is not null &&
            Parsemester is not null;

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public GroupRegistryViewModel(UserStore userStore, ApiService apiService, List<SpecialtyInfo> specialtiesInfo,
            IRelayCommand closeCommand, GroupWithSpecialtyInfo? groupInfo = null)
        {
            _userStore = userStore;
            _apiService = apiService;

            CloseCommand = closeCommand;
            SubmitCommand = groupInfo is null ? AddGroupCommand : UpdateGroupCommand;

            Header = groupInfo is null ? "Додати групу" : "Редагувати групу";

            Specialties = specialtiesInfo;

            EduLevels = new List<EduLevelInfo>()
            {
                new EduLevelInfo() { EduLevelID = 1, LevelName = "Бакалавр" },
                new EduLevelInfo() { EduLevelID = 2, LevelName = "Магістр" },
                new EduLevelInfo() { EduLevelID = 3, LevelName = "PHD" },
            };

            _groupId = groupInfo?.GroupId ?? 0;
            GroupCode = groupInfo?.GroupCode;
            Specialty = Specialties.FirstOrDefault(sp => sp.SpecialtyId == groupInfo?.Specialty.SpecialtyId);
            Course = groupInfo?.Course;
            EduLevel = EduLevels.FirstOrDefault(level => level.EduLevelID == groupInfo?.EduLevel);
            Nonparsemester = groupInfo?.Nonparsemester;
            Parsemester = groupInfo?.Parsemester;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddGroup()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newGroup = new GroupWithSpecialtyInfo
                {
                    GroupId = _groupId,
                    GroupCode = GroupCode,
                    Specialty = Specialty,
                    Course = Course.Value,
                    EduLevel = EduLevel.EduLevelID,
                    Nonparsemester = Nonparsemester.Value,
                    Parsemester = Parsemester.Value
                };

                (ErrorMessage, var addedGroup) =
                    await _apiService.PostAsync<GroupWithSpecialtyInfo>("Group", "addGroup", newGroup, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedGroup);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateGroup()
        {
            await ExecuteWithWaiting(async () =>
            {
                var updatedGroup = new GroupWithSpecialtyInfo
                {
                    GroupId = _groupId,
                    GroupCode = GroupCode,
                    Specialty = Specialty,
                    Course = Course.Value,
                    EduLevel = EduLevel.EduLevelID,
                    Nonparsemester = Nonparsemester.Value,
                    Parsemester = Parsemester.Value
                };

                (ErrorMessage, _) =
                    await _apiService.PutAsync<GroupWithSpecialtyInfo>("Group", "updateGroup", updatedGroup, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedGroup);
            });
        }

        private void OnSubmitAccepted(GroupWithSpecialtyInfo groupInfo)
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
    }
}
