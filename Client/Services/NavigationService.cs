using Client.Stores.NavigationStores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.Services
{
    public class NavigationService<TViewModel> where TViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<TViewModel> _createViewModel;

        public NavigationService(NavigationStore navigationStore, Func<TViewModel> createViewModel)
        {
            _navigationStore = navigationStore;
            _createViewModel = createViewModel;
        }

        public void Navigate()
        {
            _navigationStore.CurrentViewModel = (IPageViewModel)_createViewModel();
        }
    }
}
