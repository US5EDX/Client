using Client.Stores.NavigationStores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Services
{
    public class FrameNavigationService<TViewModel> where TViewModel : ObservableObject
    {
        private readonly FrameNavigationStore _frameNavigationStore;
        private readonly Func<TViewModel> _createFrameViewModel;

        public event Func<string, Task> OnNavigationRequested = null!;

        public FrameNavigationService(FrameNavigationStore frameNavigationStore, Func<TViewModel> createFrameViewModel)
        {
            _frameNavigationStore = frameNavigationStore;
            _createFrameViewModel = createFrameViewModel;
        }

        public void Navigate()
        {
            _frameNavigationStore.CurrentFrameViewModel = (IPageViewModel)_createFrameViewModel();
        }

        public async Task NavigateAsync()
        {
            var viewModel = (IFrameViewModel)_createFrameViewModel();
            await viewModel.LoadContentAsync();
            _frameNavigationStore.CurrentFrameViewModel = viewModel;
        }

        public void RequestNavigation(string destination)
        {
            OnNavigationRequested?.Invoke(destination);
        }
    }
}
