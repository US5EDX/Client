using Client.ViewModels.Interfaces;

namespace Client.Stores.NavigationStores
{
    public class FrameNavigationStore
    {
        private IPageViewModel _currentFrameViewModel;
        public IPageViewModel CurrentFrameViewModel
        {
            get => _currentFrameViewModel;
            set
            {
                _currentFrameViewModel = value;
                OnCurrentFrameViewModelChanged();
            }
        }

        public event Action CurrentFrameViewModelChanged;

        public FrameNavigationStore()
        {
            _currentFrameViewModel = default!;
            CurrentFrameViewModelChanged = default!;
        }

        private void OnCurrentFrameViewModelChanged()
        {
            CurrentFrameViewModelChanged?.Invoke();
        }
    }
}
