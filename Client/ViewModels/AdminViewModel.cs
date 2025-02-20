using Client.Services;
using Client.Stores.NavigationStores;
using Client.ViewModels.Interfaces;
using Client.ViewModels.NavigationViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels
{
    public partial class AdminViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly FrameNavigationViewModel _frameNavigation;
        private readonly FrameNavigationStore _frameNavigationStore;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string _errorMessage = default!;

        private string _lastAttemptedDestination;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public IPageViewModel CurrentFrameViewModel => _frameNavigationStore.CurrentFrameViewModel;

        public AdminViewModel(FrameNavigationStore frameNavigationStore, FrameNavigationViewModel frameNavigation,
            FrameNavigationService<GroupPageViewModel> groupNavigationService,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService)
        {
            _frameNavigationStore = frameNavigationStore;
            _frameNavigationStore.CurrentFrameViewModelChanged += OnCurrentFrameViewModelChanged;
            _frameNavigation = frameNavigation;
            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;

            _groupNavigationService = groupNavigationService;
            _groupNavigationService.OnNavigationRequested += Navigate;
            _allStudentCohicesNavigationService.OnNavigationRequested += Navigate;

            Task.Run(async () => await LoadHomeOnStart());
        }

        protected override void OnDeactivated()
        {
            _frameNavigationStore.CurrentFrameViewModelChanged -= OnCurrentFrameViewModelChanged;
            _groupNavigationService.OnNavigationRequested -= Navigate;
            _allStudentCohicesNavigationService.OnNavigationRequested -= Navigate;

            base.OnDeactivated();
        }

        private void OnCurrentFrameViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentFrameViewModel));
        }

        private async Task LoadHomeOnStart()
        {
            IsLoading = true;
            _lastAttemptedDestination = "Home";
            try
            {
                await _frameNavigation.AdminNavigate("Home");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Не вдалося завантажити домашню сторінку:\n{ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Navigate(string destination)
        {
            ErrorMessage = string.Empty;
            IsLoading = true;
            _lastAttemptedDestination = destination;
            try
            {
                await _frameNavigation.AdminNavigate(destination);
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
    }
}
