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
    public partial class GroupsPageViewModel : FrameBaseViewModelWithModal
    {
        private readonly IMessageService _messenger;
        private readonly GroupInfoStore _groupStore;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;

        private readonly List<SpecialtyInfo> _specialtiesInfo;

        public ObservableCollection<GroupFullInfo> Groups { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGroupSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteGroupCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateCommand))]
        private GroupFullInfo? _selectedGroup;

        public bool IsGroupSelected => SelectedGroup is not null;

        public Func<object, string, bool> Filter { get; init; }

        public bool IsAdmin => _userStore.Role == 2;

        public GroupsPageViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupStore,
            IMessageService messenger, FrameNavigationService<GroupPageViewModel> groupNavigationService) :
            base(apiService, userStore)
        {
            if (_userStore.Role > 3)
                throw new Exception("доступ обмежено");

            _messenger = messenger;
            _groupStore = groupStore;
            _groupNavigationService = groupNavigationService;

            _specialtiesInfo = [];
            Groups = [];

            WeakReferenceMessenger.Default.Register<GroupUpdatedMessage>(this, Receive);

            Filter = FilterGroups;
        }

        public override async Task LoadContentAsync()
        {
            if (IsAdmin)
                await LoadSpecialtiesAsync();

            (ErrorMessage, var groups) =
                await _apiService.GetAsync<ObservableCollection<GroupFullInfo>>
                ("Group", $"getByFacultyId?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}",
                _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var group in groups ?? Enumerable.Empty<GroupFullInfo>())
                Groups.Add(group);
        }

        public void Receive(object recipient, GroupUpdatedMessage message)
        {
            GroupFullInfo groupInfo = message.Value;

            if (IsGroupSelected && SelectedGroup.GroupId == groupInfo.GroupId)
                SelectedGroup.UpdateInfo(groupInfo);
            else
                Groups.Add(groupInfo);

            SelectedGroup = null;
        }

        private async Task LoadSpecialtiesAsync()
        {
            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<List<SpecialtyInfo>>
                ("Specialty", $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _specialtiesInfo.AddRange(specialties ?? Enumerable.Empty<SpecialtyInfo>());
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new GroupRegistryViewModel(_userStore, _apiService,
            _specialtiesInfo, CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private void OpenUpdateModal() => SelectedModal = new GroupRegistryViewModel(_userStore, _apiService,
            _specialtiesInfo, CloseModalCommand, SelectedGroup);

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private async Task DeleteGroup()
        {
            bool isOk = _messenger.ShowQuestion($"Ви дійсно хочете видалити групу {SelectedGroup.GroupCode}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Group", $"deleteGroup/{SelectedGroup.GroupId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Groups.Remove(SelectedGroup);
                    SelectedGroup = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsGroupSelected))]
        private void Navigate()
        {
            _groupStore.GroupId = SelectedGroup.GroupId;
            _groupStore.GetInfoFromModel(SelectedGroup.ToGroupInfo());

            _groupNavigationService.RequestNavigation("Group");
        }

        private bool FilterGroups(object group, string filter)
        {
            if (group is not GroupFullInfo groupInfo) return false;

            return groupInfo.GroupCode.Contains(filter, StringComparison.OrdinalIgnoreCase)
                || (groupInfo.CuratorInfo is not null ?
                groupInfo.CuratorInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) :
                "Без куратора".Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
    }
}
