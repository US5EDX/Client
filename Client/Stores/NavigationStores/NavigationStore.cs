using Client.ViewModels.Interfaces;

namespace Client.Stores.NavigationStores
{
    public class NavigationStore
    {
        private IPageViewModel _currentViewModel;
        public IPageViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != null)
                {
                    _currentViewModel.IsActive = false;
                }

                _currentViewModel = value;

                if (_currentViewModel != null)
                {
                    _currentViewModel.IsActive = true;
                }

                OnCurrentViewModelChanged();
            }
        }

        public event Action CurrentViewModelChanged;

        public NavigationStore()
        {
            _currentViewModel = default!;
            CurrentViewModelChanged = default!;
        }

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
