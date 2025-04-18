using Client.Models;
using Client.PdfDoucments;
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
        private readonly StudentsReaderService _studentsReaderService;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;
        private readonly StudentInfoStore _studentInfoStore;

        private readonly ObservableCollection<StudentRecordsInfo> _students;

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        public IEnumerable<StudentRecordsInfo> Students => _students;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStudentSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteStudentCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToStudentCommand))]
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

        public GroupPageViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            IMessageService messageService, StudentsReaderService studentsReaderService,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService,
            StudentInfoStore studentInfoStore)
        {
            _apiService = apiService;
            _userStore = userStore;
            _groupInfoStore = groupInfoStore;
            _messageService = messageService;
            _studentsReaderService = studentsReaderService;
            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;
            _studentInfoStore = studentInfoStore;

            _students = new ObservableCollection<StudentRecordsInfo>();

            WeakReferenceMessenger.Default.Register<StudentUpdatedMessage>(this, Receive);
            WeakReferenceMessenger.Default.Register<StudentsAddedMessage>(this, ReceiveNewStudents);

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

        public void ReceiveNewStudents(object recipient, StudentsAddedMessage message)
        {
            SelectedStudent = null;

            IEnumerable<StudentRegistryInfo> students = message.Value;

            foreach (var student in students)
                _students.Add(student.ToStudentWithRecords());
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

        [RelayCommand(CanExecute = nameof(IsStudentSelected))]
        public void NavigateToStudent()
        {
            _studentInfoStore.StudentId = SelectedStudent.StudentId;
            _studentInfoStore.FullName = SelectedStudent.FullName;
            _studentInfoStore.GroupId = _groupInfoStore.GroupId;
            _groupInfoStore.IsLoadedFromGroupsPage = true;

            _allStudentCohicesNavigationService.RequestNavigation("AllChoices");
        }

        [RelayCommand]
        private async Task GeneratePdf()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            var path = _messageService.ShowSaveFileDialog("Вибіріть місце збереження", "Pdf file|*.pdf");

            if (path is null)
            {
                IsWaiting = false;
                return;
            }

            var reportDocument = new StudentsRecordsDocument(_students, Header, NonparsemesterCount, ParsemesterCount);
            ErrorMessage = await PdfGenerator.GeneratePdf(reportDocument, path);

            IsWaiting = false;
        }

        [RelayCommand]
        private async Task LoadFromFile()
        {
            await ExecuteWithWaiting(async () =>
            {
                var path = _messageService.ShowOpenFileDialog("Оберіть файл зі списком студентів", "Excel files|*.xlsx;*.xlsm");

                if (path == null)
                    return;

                List<StudentExcelInfo> studentsList;

                try
                {
                    studentsList = _studentsReaderService.GetStudentsInfo(path);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return;
                }

                var newStudents = studentsList.Where(st =>
                _students.FirstOrDefault(s => s.Email == st.Email) is null)
                    .Select(st => new StudentRegistryInfo()
                    {
                        Email = st.Email,
                        FullName = st.FullName,
                        Faculty = _userStore.WorkerInfo?.Faculty.FacultyId ?? _userStore.StudentInfo.Faculty.FacultyId,
                        Group = _groupInfoStore.GroupId,
                        Headman = false,
                    });

                if (!newStudents.Any())
                {
                    ErrorMessage = "Немає кого додавати";
                    return;
                }

                SelectedModal = new NewStudentsViewModel(_apiService, _userStore, newStudents, CloseModalCommand);
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
