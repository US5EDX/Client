using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Client.ViewModels
{
    public partial class NewStudentsViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;

        public IEnumerable<StudentRegistryInfo> NewStudents { get; init; }

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public IRelayCommand CloseCommand { get; init; }

        public NewStudentsViewModel(ApiService apiService, UserStore userStore,
            IEnumerable<StudentRegistryInfo> newStudents, IRelayCommand closeCommand)
        {
            _apiService = apiService;
            _userStore = userStore;
            NewStudents = newStudents;
            CloseCommand = closeCommand;
        }

        [RelayCommand]
        private async Task AddNewStudents()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            (ErrorMessage, var addedStudents) =
                    await _apiService.PostAsync<IEnumerable<StudentRegistryInfo>>("Student", "addStudents",
                    NewStudents, _userStore.AccessToken);

            if (!HasErrorMessage)
                OnSubmitAccepted(addedStudents);

            IsWaiting = false;
        }

        private void OnSubmitAccepted(IEnumerable<StudentRegistryInfo> studentsInfo)
        {
            WeakReferenceMessenger.Default.Send(new StudentsAddedMessage(studentsInfo));
            CloseCommand.Execute(null);
        }
    }
}
