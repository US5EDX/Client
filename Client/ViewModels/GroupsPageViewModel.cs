using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.Stores.Messangers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class GroupsPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messenger;
        private readonly GroupInfoStore _groupStore;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;

        private readonly ObservableCollection<GroupFullInfo> _groups;
        private readonly List<SpecialtyInfo> _specialtiesInfo;

        public IEnumerable<GroupFullInfo> Groups => _groups;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegistryOpen))]
        private GroupRegistryViewModel? _groupRegistry;

        public bool IsRegistryOpen => GroupRegistry is not null;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateCommand))]
        private GroupFullInfo? _selectedGroup;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsGroupSelected => SelectedGroup is not null;

        public Func<object, string, bool> Filter { get; init; }

        public GroupsPageViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupStore,
            IMessageService messenger, FrameNavigationService<GroupPageViewModel> groupNavigationService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messenger = messenger;
            _groupStore = groupStore;
            _groupNavigationService = groupNavigationService;

            _groups = new ObservableCollection<GroupFullInfo>();
            _specialtiesInfo = new List<SpecialtyInfo>();

            WeakReferenceMessenger.Default.Register<GroupUpdatedMessage>(this, Receive);

            GroupRegistry = null;
            Filter = FilterGroups;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var groups) =
                await _apiService.GetAsync<ObservableCollection<GroupFullInfo>>
                ("Group", $"getByFacultyId?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _groups.Clear();

            foreach (var group in groups ?? Enumerable.Empty<GroupFullInfo>())
                _groups.Add(group);

            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<List<SpecialtyInfo>>
                ("Specialty", $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _specialtiesInfo.Clear();

            foreach (var specialty in specialties ?? Enumerable.Empty<SpecialtyInfo>())
                _specialtiesInfo.Add(specialty);
        }

        [RelayCommand]
        private void CloseModal()
        {
            GroupRegistry = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            GroupRegistry = new GroupRegistryViewModel(_userStore, _apiService, _specialtiesInfo, CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private void OpenUpdateModal()
        {
            GroupRegistry = new GroupRegistryViewModel(_userStore, _apiService, _specialtiesInfo, CloseModalCommand, SelectedGroup);
        }

        public void Receive(object recipient, GroupUpdatedMessage message)
        {
            GroupFullInfo groupInfo = message.Value;

            if (IsGroupSelected && SelectedGroup.GroupId == groupInfo.GroupId)
            {
                SelectedGroup.GroupCode = groupInfo.GroupCode;
                SelectedGroup.Specialty = groupInfo.Specialty;
                SelectedGroup.Course = groupInfo.Course;
                SelectedGroup.EduLevel = groupInfo.EduLevel;
                SelectedGroup.DurationOfStudy = groupInfo.DurationOfStudy;
                SelectedGroup.AdmissionYear = groupInfo.AdmissionYear;
                SelectedGroup.Nonparsemester = groupInfo.Nonparsemester;
                SelectedGroup.Parsemester = groupInfo.Parsemester;
                SelectedGroup.HasEnterChoise = groupInfo.HasEnterChoise;
                SelectedGroup.CuratorInfo = groupInfo.CuratorInfo;
                SelectedGroup = null;
                return;
            }

            _groups.Add(groupInfo);
            SelectedGroup = null;
        }

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private async Task DeleteGroup()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Group", $"deleteGroup/{SelectedGroup.GroupId}", _userStore.AccessToken);

            if (!HasErrorMessage)
            {
                _groups.Remove(SelectedGroup);
                SelectedGroup = null;
            }

            IsWaiting = false;
        }

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private async Task Navigate()
        {
            _groupStore.GroupId = SelectedGroup.GroupId;
            _groupStore.GetInfoFromModel(SelectedGroup.ToGroupInfo());
            _groupStore.IsLoadedFromGroupsPage = true;

            _groupNavigationService.RequestNavigation("Group");
        }

        private bool FilterGroups(object group, string filter)
        {
            if (group is not GroupFullInfo groupInfo)
                return false;

            return groupInfo.GroupCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || (groupInfo.CuratorInfo is not null ?
                groupInfo.CuratorInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) :
                "Без куратора".Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
    }
}
