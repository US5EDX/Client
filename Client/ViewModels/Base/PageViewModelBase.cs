using Client.Stores.NavigationStores;
using Client.ViewModels.Interfaces;
using Client.ViewModels.NavigationViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base.PageBase
{
    public abstract partial class PageViewModelBase(SuccsefulLoginViewModel succsefulLoginViewModel,
        FrameNavigationStore frameNavigationStore) : ObservableRecipient, IPageViewModel
    {
        protected readonly SuccsefulLoginViewModel _succsefulLoginViewModel = succsefulLoginViewModel;
        protected readonly FrameNavigationStore _frameNavigationStore = frameNavigationStore;

        protected string? _lastAttemptedDestination;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        protected Func<string, Task> ChangeFrame { get; init; } = null!;

        public IPageViewModel CurrentFrameViewModel => _frameNavigationStore.CurrentFrameViewModel;

        protected void OnCurrentFrameViewModelChanged() => OnPropertyChanged(nameof(CurrentFrameViewModel));

        [RelayCommand]
        protected virtual async Task Navigate(string destination)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            _lastAttemptedDestination = destination;

            try
            {
                await ChangeFrame(destination);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не вдалося завантажити {(destination == "Home" ? "домашню" : string.Empty)} сторінку:\n{ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RetryNavigation()
        {
            if (HasErrorMessage && !string.IsNullOrEmpty(_lastAttemptedDestination))
                await Navigate(_lastAttemptedDestination);
        }

        [RelayCommand]
        private async Task Logout() => ErrorMessage = await _succsefulLoginViewModel.Logout();
    }
}
