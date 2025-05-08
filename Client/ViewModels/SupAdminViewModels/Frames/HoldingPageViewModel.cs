using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class HoldingPageViewModel : FrameBaseViewModelWithModal
    {
        public ObservableCollection<HoldingInfo> Holdings { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHoldingSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteHoldingCommand))]
        private HoldingInfo? _selectedHolding;

        public bool IsHoldingSelected => SelectedHolding is not null;

        public HoldingPageViewModel(ApiService apiService, UserStore userStore) : base(apiService, userStore)
        {
            Holdings = [];
            WeakReferenceMessenger.Default.Register<HoldingUpdatedMessage>(this, Receive);
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var holdings) =
                await _apiService.GetAsync<ObservableCollection<HoldingInfo>>("Holding", "getHoldings", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var holding in holdings ?? Enumerable.Empty<HoldingInfo>())
                Holdings.Add(holding);
        }

        public void Receive(object recipient, HoldingUpdatedMessage message)
        {
            HoldingInfo holdingInfo = message.Value;

            if (IsHoldingSelected && SelectedHolding.EduYear == holdingInfo.EduYear)
                SelectedHolding.UpdateInfo(holdingInfo);
            else
                Holdings.Add(holdingInfo);

            SelectedHolding = null;
        }

        [RelayCommand]
        private void OpenAddModal() => SelectedModal = new HoldingRegistryViewModel(_userStore, _apiService, CloseModalCommand);

        [RelayCommand(CanExecute = nameof(IsHoldingSelected))]
        private void OpenUpdateModal() => SelectedModal = new HoldingRegistryViewModel(_userStore, _apiService,
            CloseModalCommand, SelectedHolding);

        [RelayCommand(CanExecute = nameof(IsHoldingSelected))]
        private async Task DeleteHolding()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Holding", $"deleteHolding/{SelectedHolding.EduYear}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Holdings.Remove(SelectedHolding);
                    SelectedHolding = null;
                }
            });
        }
    }
}
