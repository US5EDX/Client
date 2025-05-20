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
    public partial class WorkerRegistryViewModel : ViewModelBaseValidationExtended
    {
        private readonly string? _id;

        public List<FacultyInfo> Faculties { get; init; }
        public List<RoleInfo> Roles { get; init; }

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [EmailAddress]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string _email = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private RoleInfo? _role;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(2, 150)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string _fullName = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private FacultyInfo? _faculty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(2, 255)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string _department = null!;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required]
        [Length(2, 100)]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private string _position = null!;

        public override bool CanSubmit => !HasErrors &&
            Role is not null &&
            Faculty is not null;

        public WorkerRegistryViewModel(UserStore userStore, ApiService apiService, IEnumerable<FacultyInfo> facultiesInfo,
            IRelayCommand closeCommand, UserFullInfo? workerInfo = null) :
            base(apiService, userStore, closeCommand)
        {
            SubmitCommand = workerInfo is null ? AddCommand : UpdateCommand;

            if (_userStore.Role > 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            Faculties = [.. facultiesInfo];

            Roles = [];

            IsAddMode = workerInfo is null;

            _id = workerInfo?.Id;

            InithializeProperties(workerInfo);

            if (_userStore.Role == 2)
            {
                Header = workerInfo is null ? "Додати викладача" : "Редагувати інформацію про викладача";

                Roles.Add(new RoleInfo { RoleId = 3, RoleName = "Викладач" });
                _role = Roles[0];

                return;
            }

            Header = workerInfo is null ? "Додати адміністратора" : "Редагувати інформацію про адмістратора";

            Roles.Add(new RoleInfo { RoleId = 2, RoleName = "Адміністратор" });

            if (workerInfo is not null)
                Roles.Add(new RoleInfo { RoleId = 3, RoleName = "Викладач" });

            int role = workerInfo?.Role ?? 2;
            _role = Roles[role - 2];
        }

        protected override async Task Add()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var newWorker = InithializeInstance();

                (ErrorMessage, var addedWorker) =
                    await _apiService.PostAsync<UserFullInfo>("Worker", "addWorker", newWorker, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedWorker);
            });
        }

        protected override async Task Update()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var updatingWorker = InithializeInstance();

                (ErrorMessage, var updatedWorker) =
                    await _apiService.PutAsync<UserFullInfo>("Worker", "updateWorker", updatingWorker, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedWorker);
            });
        }

        private void OnSubmitAccepted(UserFullInfo workerInfo)
        {
            WeakReferenceMessenger.Default.Send(new WorkerUpdatedMessage(workerInfo));
            CloseCommand.Execute(null);
        }

        private void InithializeProperties(UserFullInfo? workerInfo)
        {
            _email = workerInfo?.Email ?? string.Empty;
            _fullName = workerInfo?.FullName ?? string.Empty;
            _faculty = Faculties.FirstOrDefault(f => f.FacultyId == workerInfo?.Faculty.FacultyId);
            _department = workerInfo?.Department ?? string.Empty;
            _position = workerInfo?.Position ?? string.Empty;
        }

        private WorkerRegistryInfo InithializeInstance()
        {
            return new WorkerRegistryInfo
            {
                WorkerId = _id,
                Email = Email,
                Role = Role.RoleId,
                FullName = FullName,
                FacultyId = Faculty.FacultyId,
                Department = Department,
                Position = Position,
            };
        }
    }
}
