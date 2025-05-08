using Client.Services;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base
{
    public abstract partial class FrameBaseViewModelWithModal(ApiService apiService, UserStore userStore) :
        FrameBaseViewModel(apiService, userStore)
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsModalOpen))]
        private IPageViewModel? _selectedModal;

        public bool IsModalOpen => SelectedModal is not null;

        [RelayCommand]
        protected void CloseModal() => SelectedModal = null;
    }
}
