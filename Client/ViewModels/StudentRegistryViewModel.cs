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
    public partial class StudentRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly GroupInfoStore _groupInfoStore;

        private readonly string? _studentId;

        private readonly uint _facultyId;

        private readonly uint _groupId;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [EmailAddress]
        [Length(1, 255)]
        private string _email;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 150)]
        private string _fullName;

        [ObservableProperty]
        private bool _headman;

        public string Header { get; init; }

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !HasErrors;

        public IRelayCommand CloseCommand { get; set; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public StudentRegistryViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            IRelayCommand closeCommand, StudentRecordsInfo? student = null)
        {
            _apiService = apiService;
            _userStore = userStore;
            _groupInfoStore = groupInfoStore;

            CloseCommand = closeCommand;
            SubmitCommand = student is null ? AddStudentCommand : UpdateStudentCommand;

            Header = student is null ? "Додати студента" : "Оновити інформацію про студента";

            _studentId = student?.StudentId;
            _facultyId = _userStore.Role < 4 ? _userStore.WorkerInfo.Faculty.FacultyId : _userStore.StudentInfo.Faculty.FacultyId;
            _groupId = _groupInfoStore.GroupId;
            _email = student?.Email;
            _fullName = student?.FullName;
            _headman = student?.Headman ?? false;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddStudent()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newStudent = CreateNewInfo();

                (ErrorMessage, var addedStudent) =
                    await _apiService.PostAsync<StudentRegistryInfo>("Student", "addStudent", newStudent, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedStudent);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateStudent()
        {
            await ExecuteWithWaiting(async () =>
            {
                var updatedstudent = CreateNewInfo();

                (ErrorMessage, _) =
                    await _apiService.PutAsync<GroupWithSpecialtyInfo>("Student", "updateStudent", updatedstudent, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedstudent);
            });
        }

        private StudentRegistryInfo CreateNewInfo()
        {
            return new StudentRegistryInfo
            {
                StudentId = _studentId,
                Email = Email,
                FullName = FullName,
                Headman = Headman,
                Faculty = _facultyId,
                Group = _groupId,
            };
        }

        private void OnSubmitAccepted(StudentRegistryInfo studentInfo)
        {
            WeakReferenceMessenger.Default.Send(new StudentUpdatedMessage(studentInfo));
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
