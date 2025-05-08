using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    public partial class StudentRegistryViewModel : ViewModelBaseWithValidation
    {
        private readonly GroupInfoStore _groupInfoStore;

        private readonly string? _studentId;

        private readonly uint _facultyId;

        private readonly uint _groupId;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [EmailAddress]
        [Length(1, 255)]
        private string? _email;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(1, 150)]
        private string? _fullName;

        [ObservableProperty]
        private bool _headman;

        public override bool CanSubmit => !HasErrors;

        public StudentRegistryViewModel(ApiService apiService, UserStore userStore, GroupInfoStore groupInfoStore,
            IRelayCommand closeCommand, StudentRecordsInfo? student = null) : base(apiService, userStore, closeCommand)
        {
            _groupInfoStore = groupInfoStore;

            SubmitCommand = student is null ? AddCommand : UpdateCommand;

            Header = student is null ? "Додати студента" : "Оновити інформацію про студента";

            _studentId = student?.StudentId;
            _facultyId = _userStore.Role < 4 ? _userStore.WorkerInfo.Faculty.FacultyId : _userStore.StudentInfo.Faculty.FacultyId;
            _groupId = _groupInfoStore.GroupId;
            _email = student?.Email;
            _fullName = student?.FullName;
            _headman = student?.Headman ?? false;
        }

        protected override async Task Add()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var newStudent = InithializeInstance();

                (ErrorMessage, var addedStudent) =
                    await _apiService.PostAsync<StudentRegistryInfo>("Student", "addStudent", newStudent, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedStudent);
            });
        }

        protected override async Task Update()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var updatedstudent = InithializeInstance();

                (ErrorMessage, _) =
                    await _apiService.PutAsync<StudentRegistryInfo>("Student", "updateStudent", updatedstudent, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedstudent);
            });
        }

        private void OnSubmitAccepted(StudentRegistryInfo studentInfo)
        {
            WeakReferenceMessenger.Default.Send(new StudentUpdatedMessage(studentInfo));
            CloseCommand.Execute(null);
        }

        private StudentRegistryInfo InithializeInstance()
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
    }
}
