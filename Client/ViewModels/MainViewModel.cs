using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly NavigationStore _navigationStore;

        public IPageViewModel CurrentViewModel => _navigationStore.CurrentViewModel;

        public MainViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
