using Client.Services;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base
{
    [ObservableRecipient]
    public abstract partial class ViewModelBaseWithValidation(ApiService apiService, UserStore userStore,
        IRelayCommand closeCommand) : ObservableValidator, IPageViewModel
    {
        protected readonly ApiService _apiService = apiService;
        protected readonly UserStore _userStore = userStore;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotLocked))]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        private string _header = null!;

        public bool IsNotLocked => !IsWaiting;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsAddMode { get; init; }

        public abstract bool CanSubmit { get; }

        public IRelayCommand CloseCommand { get; init; } = closeCommand;

        public IAsyncRelayCommand SubmitCommand { get; init; } = null!;

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        protected abstract Task Add();

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        protected abstract Task Update();

        protected async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }
    }
}
