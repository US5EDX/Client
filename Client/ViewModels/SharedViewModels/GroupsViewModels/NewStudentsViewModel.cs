using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Client.ViewModels
{
    public partial class NewStudentsViewModel(ApiService apiService, UserStore userStore,
        IEnumerable<StudentRegistryInfo> newStudents, IRelayCommand closeCommand) : ViewModelBase(apiService, userStore)
    {
        public IEnumerable<StudentRegistryInfo> NewStudents { get; init; } = newStudents;

        public IRelayCommand CloseCommand { get; init; } = closeCommand;

        [RelayCommand]
        private async Task AddNewStudents()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var addedStudents) =
                    await _apiService.PostAsync<IEnumerable<StudentRegistryInfo>>("Student", "addStudents",
                    NewStudents, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedStudents);
            });
        }

        private void OnSubmitAccepted(IEnumerable<StudentRegistryInfo> studentsInfo)
        {
            WeakReferenceMessenger.Default.Send(new StudentsAddedMessage(studentsInfo));
            CloseCommand.Execute(null);
        }
    }
}
