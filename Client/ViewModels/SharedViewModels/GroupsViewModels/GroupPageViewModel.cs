using Client.ExcelDocuments;
using Client.Models;
using Client.PdfDoucments;
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
    public partial class GroupPageViewModel : FrameBaseViewModelWithModal
    {
        private readonly GroupInfoStore _groupInfoStore;
        private readonly IMessageService _messageService;
        private readonly StudentsReaderService _studentsReaderService;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;
        private readonly StudentInfoStore _studentInfoStore;

        public string Header { get; set; } = null!;

        public ObservableCollection<StudentRecordsInfo> Students { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsStudentSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteStudentCommand))]
        [NotifyCanExecuteChangedFor(nameof(NavigateToStudentCommand))]
        private StudentRecordsInfo? _selectedStudent;

        public byte NonparsemesterCount => _groupInfoStore.Nonparsemester;
        public byte ParsemesterCount => _groupInfoStore.Parsemester;

        public bool IsStudentSelected => SelectedStudent is not null;

        public Func<object, string, bool> Filter { get; init; }

        public GroupPageViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            IMessageService messageService, StudentsReaderService studentsReaderService,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService,
            StudentInfoStore studentInfoStore) : base(apiService, userStore)
        {
            _groupInfoStore = groupInfoStore;
            _messageService = messageService;
            _studentsReaderService = studentsReaderService;
            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;
            _studentInfoStore = studentInfoStore;

            Students = [];

            WeakReferenceMessenger.Default.Register<StudentUpdatedMessage>(this, Receive);
            WeakReferenceMessenger.Default.Register<StudentsAddedMessage>(this, ReceiveNewStudents);

            Filter = FilterStudents;
        }

        public override async Task LoadContentAsync()
        {
            await _groupInfoStore.LoadInfoAsync(_apiService, _groupInfoStore.GroupId, _userStore.AccessToken);

            Header = _groupInfoStore.GroupCode;

            (ErrorMessage, var students) =
                    await _apiService.GetAsync<List<StudentRecordsInfo>>
                    ("Student", $"getWithRecrodsByGroupId/{_groupInfoStore.GroupId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var student in students ?? Enumerable.Empty<StudentRecordsInfo>())
                Students.Add(student);
        }

        public void Receive(object recipient, StudentUpdatedMessage message)
        {
            StudentRegistryInfo student = message.Value;

            if (IsStudentSelected && SelectedStudent.StudentId == student.StudentId)
                SelectedStudent.UpdateInfo(student);
            else
                Students.Add(student.ToStudentWithRecords());

            SelectedStudent = null;
        }

        public void ReceiveNewStudents(object recipient, StudentsAddedMessage message)
        {
            SelectedStudent = null;

            IEnumerable<StudentRegistryInfo> students = message.Value;

            foreach (var student in students)
                Students.Add(student.ToStudentWithRecords());
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new StudentRegistryViewModel(_apiService, _userStore,
            _groupInfoStore, CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsStudentSelected))]
        private void OpenUpdateModal() => SelectedModal = new StudentRegistryViewModel(_apiService, _userStore,
            _groupInfoStore, CloseModalCommand, SelectedStudent);

        [RelayCommand(CanExecute = nameof(IsStudentSelected))]
        private async Task DeleteStudent()
        {
            bool isOk = _messageService.ShowQuestion($"Ви дійсно хочете видалити студента {SelectedStudent.FullName}");

            if (!isOk) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Student", $"deleteStudent/{SelectedStudent.StudentId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Students.Remove(SelectedStudent);
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

            _allStudentCohicesNavigationService.RequestNavigation("AllChoices");
        }

        [RelayCommand]
        private async Task GeneratePdf()
        {
            var path = _messageService.ShowSaveFileDialog("Вибіріть місце збереження", "Pdf file|*.pdf");

            if (path is null) return;

            var reportDocument = new StudentsRecordsDocument(Students, Header, NonparsemesterCount, ParsemesterCount);

            await ExecuteWithWaiting(async () => ErrorMessage = await PdfGenerator.GeneratePdf(reportDocument, path));
        }

        [RelayCommand]
        private async Task LoadFromFile()
        {
            var path = _messageService.ShowOpenFileDialog("Оберіть файл зі списком студентів", "Excel files|*.xlsx;*.xlsm");

            if (path == null) return;

            await ExecuteWithWaiting(async () =>
            {
                List<StudentExcelInfo> studentsList = null!;

                try
                {
                    studentsList = await _studentsReaderService.GetStudentsInfoAsync(path);
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return;
                }

                var newStudents = studentsList.Where(st =>
                Students.FirstOrDefault(s => s.Email == st.Email) is null)
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

        [RelayCommand]
        private async Task GenerateExcel()
        {
            var path = _messageService.ShowSaveFileDialog("Виберіть місце збереження відомості", "Excel file|*.xlsx");

            if (path is null) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var students) =
                        await _apiService.GetAsync<List<StudentWithAllRecordsInfo>>("Student", $"getWithAllRecrodsByGroupId/" +
                        $"{_groupInfoStore.GroupId}", _userStore.AccessToken);

                if (HasErrorMessage) return;

                var reportDocument = new StudentsRecordsExcelDocument(students, _groupInfoStore);

                ErrorMessage = await reportDocument.GenerateExcelAsync(path);
            });
        }

        private bool FilterStudents(object student, string filter)
        {
            if (student is not StudentRecordsInfo studentInfo) return false;

            return studentInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                studentInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
