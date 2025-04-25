using Client.Stores.NavigationStores;
using Client.ViewModels.Interfaces;
using Client.ViewModels.NavigationViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels
{
    public partial class StudentViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly SuccsefulLoginViewModel _succsefulLoginViewModel;
        private readonly FrameNavigationViewModel _frameNavigation;
        private readonly FrameNavigationStore _frameNavigationStore;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        private string _lastAttemptedDestination;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public IPageViewModel CurrentFrameViewModel => _frameNavigationStore.CurrentFrameViewModel;

        public StudentViewModel(SuccsefulLoginViewModel succsefulLoginViewModel,
            FrameNavigationStore frameNavigationStore, FrameNavigationViewModel frameNavigation)
        {
            _succsefulLoginViewModel = succsefulLoginViewModel;
            _frameNavigationStore = frameNavigationStore;
            _frameNavigationStore.CurrentFrameViewModelChanged += OnCurrentFrameViewModelChanged;
            _frameNavigation = frameNavigation;

            Task.Run(async () => await LoadHomeOnStart());
        }

        protected override void OnDeactivated()
        {
            _frameNavigationStore.CurrentFrameViewModelChanged -= OnCurrentFrameViewModelChanged;

            base.OnDeactivated();
        }

        private void OnCurrentFrameViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentFrameViewModel));
        }

        private async Task LoadHomeOnStart()
        {
            await Navigate("Home");
        }

        [RelayCommand]
        private async Task Navigate(string destination)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;
            _lastAttemptedDestination = destination;
            try
            {
                await _frameNavigation.StudentNavigate(destination);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не вдалося завантажити сторінку:\n{ex.Message}";
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
        private async Task Logout()
        {
            ErrorMessage = await _succsefulLoginViewModel.Logout();
        }
    }
}
