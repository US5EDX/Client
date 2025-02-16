using Client.Models;
using Client.Services;
using Client.Stores.Messangers;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Services.MessageService;

namespace Client.ViewModels
{
    public partial class GroupsPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly IMessageService _messenger;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;

        private readonly ObservableCollection<GroupWithSpecialtyInfo> _groups;
        private readonly List<SpecialtyInfo> _specialtiesInfo;

        public IEnumerable<GroupWithSpecialtyInfo> Groups => _groups;

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
        private GroupWithSpecialtyInfo? _selectedGroup;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsGroupSelected => SelectedGroup is not null;

        public Func<object, string, bool> Filter { get; init; }

        public GroupsPageViewModel(ApiService apiService, UserStore userStore,
            IMessageService messenger, FrameNavigationService<GroupPageViewModel> groupNavigationService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _messenger = messenger;
            _groupNavigationService = groupNavigationService;

            _groups = new ObservableCollection<GroupWithSpecialtyInfo>();
            _specialtiesInfo = new List<SpecialtyInfo>();

            WeakReferenceMessenger.Default.Register<GroupUpdatedMessage>(this, Receive);

            GroupRegistry = null;
            Filter = FilterGroups;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var groups) =
                await _apiService.GetAsync<ObservableCollection<GroupWithSpecialtyInfo>>
                ("Group", $"getByFacultyId?facultyId={_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _groups.Clear();

            foreach (var group in groups ?? Enumerable.Empty<GroupWithSpecialtyInfo>())
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
            GroupWithSpecialtyInfo groupInfo = message.Value;

            if (IsGroupSelected && SelectedGroup.GroupId == groupInfo.GroupId)
            {
                SelectedGroup.GroupCode = groupInfo.GroupCode;
                SelectedGroup.Specialty = groupInfo.Specialty;
                SelectedGroup.Course = groupInfo.Course;
                SelectedGroup.EduLevel = groupInfo.EduLevel;
                SelectedGroup.Nonparsemester = groupInfo.Nonparsemester;
                SelectedGroup.Parsemester = groupInfo.Parsemester;
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
            _groupNavigationService.RequestNavigation("Group");
        }

        [RelayCommand]
        private async Task UpdateCourse()
        {
            bool yes = _messenger.ShowQuestion("Ви впевнені, що хочете перевести групи на новий курс?");

            if (!yes)
                return;

            yes = _messenger.ShowQuestion("Ви точно впевнені?");

            if (!yes)
                return;

            (ErrorMessage, _) = await _apiService.PutAsync<object>(
                        "Group", $"updateGroupsCourse/{_userStore.WorkerInfo.Faculty.FacultyId}", null, _userStore.AccessToken);

            if (HasErrorMessage)
                return;

            foreach (var groupInfo in _groups)
            {
                if (groupInfo.Course == 0)
                    continue;

                var newCourse = groupInfo.Course + 1;

                if (newCourse == 5 || newCourse == 7 || newCourse == 13)
                    newCourse = 0;

                groupInfo.Course = (byte)newCourse;
            }
        }

        private bool FilterGroups(object group, string filter)
        {
            if (group is not GroupWithSpecialtyInfo groupInfo)
                return false;

            return groupInfo.GroupCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
