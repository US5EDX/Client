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
using System.IO;

namespace Client.ViewModels
{
    public partial class GroupPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly GroupInfoStore _groupInfoStore;
        private readonly IMessageService _messageService;

        private readonly ObservableCollection<StudentRecordsInfo> _students;

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        public IEnumerable<StudentRecordsInfo> Students => _students;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStudentSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteStudentCommand))]
        private StudentRecordsInfo _selectedStudent;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsModalOpen))]
        private IPageViewModel? _selectedModal;

        public bool IsModalOpen => SelectedModal is not null;

        public string Header { get; set; }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsStudentSelected => SelectedStudent is not null;

        public Func<object, string, bool> Filter { get; init; }

        public GroupPageViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore, IMessageService messageService)
        {
            _apiService = apiService;
            _userStore = userStore;
            _groupInfoStore = groupInfoStore;
            _messageService = messageService;

            _students = new ObservableCollection<StudentRecordsInfo>();

            WeakReferenceMessenger.Default.Register<StudentUpdatedMessage>(this, Receive);

            Filter = FilterStudents;
        }

        public async Task LoadContentAsync()
        {
            if (!_groupInfoStore.IsLoadedFromGroupsPage)
            {
                (ErrorMessage, var group) =
                    await _apiService.GetAsync<GroupInfo>
                    ("Group", $"getGroupById/{_groupInfoStore.GroupId}", _userStore.AccessToken);

                if (HasErrorMessage)
                    throw new Exception(ErrorMessage);

                if (group is null)
                    throw new InvalidDataException("Не вдалось завантажити дані про групу");

                _groupInfoStore.GetInfoFromModel(group);
            }

            Header = _groupInfoStore.GroupCode;

            (ErrorMessage, var students) =
                    await _apiService.GetAsync<List<StudentRecordsInfo>>
                    ("Student", $"getWithRecrodsByGroupId/{_groupInfoStore.GroupId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var student in students ?? Enumerable.Empty<StudentRecordsInfo>())
                _students.Add(student);
        }

        [RelayCommand]
        private void CloseModal()
        {
            SelectedModal = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            SelectedModal = new StudentRegistryViewModel(_apiService, _userStore, _groupInfoStore, CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsStudentSelected))]
        private void OpenUpdateModal()
        {
            SelectedModal = new StudentRegistryViewModel(_apiService, _userStore, _groupInfoStore, CloseModalCommand, SelectedStudent);
        }

        public void Receive(object recipient, StudentUpdatedMessage message)
        {
            StudentRegistryInfo student = message.Value;

            if (IsStudentSelected && SelectedStudent.StudentId == student.StudentId)
            {
                SelectedStudent.Email = student.Email;
                SelectedStudent.FullName = student.FullName;
                SelectedStudent.Headman = student.Headman;
                SelectedStudent = null;
                return;
            }

            _students.Add(student.ToStudentWithRecords());
            SelectedStudent = null;
        }

        [RelayCommand(CanExecute = nameof(IsStudentSelected))]
        private async Task DeleteStudent()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити студента {SelectedStudent.FullName}");

            if (!isOk)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Student", $"deleteStudent/{SelectedStudent.StudentId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _students.Remove(SelectedStudent);
                    SelectedStudent = null;
                }
            });
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private bool FilterStudents(object student, string filter)
        {
            if (student is not StudentRecordsInfo studentInfo)
                return false;

            return studentInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                studentInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
