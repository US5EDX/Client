using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base
{
    public abstract partial class ViewModelBaseValidationExtended(ApiService apiService, UserStore userStore,
        IRelayCommand closeCommand) : ViewModelBaseWithValidation(apiService, userStore)
    {
        [ObservableProperty]
        private string _header = null!;

        public bool IsAddMode { get; init; }

        public abstract bool CanSubmit { get; }

        public IRelayCommand CloseCommand { get; init; } = closeCommand;

        public IAsyncRelayCommand SubmitCommand { get; init; } = null!;

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        protected abstract Task Add();

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        protected abstract Task Update();
    }
}
