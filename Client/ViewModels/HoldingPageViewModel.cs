using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class HoldingPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly ObservableCollection<HoldingInfo> _holdings;
        public IEnumerable<HoldingInfo> Holdings => _holdings;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegistryOpen))]
        private HoldingRegistryViewModel? _holdingRegistry;

        public bool IsRegistryOpen => HoldingRegistry is not null;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHoldingSelected))]
        [NotifyCanExecuteChangedFor(nameof(OpenUpdateModalCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteHoldingCommand))]
        private HoldingInfo? _selectedHolding;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);
        public bool IsHoldingSelected => SelectedHolding is not null;

        public HoldingPageViewModel(ApiService apiService, UserStore userStore)
        {
            _apiService = apiService;
            _userStore = userStore;
            _holdings = new ObservableCollection<HoldingInfo>();
            WeakReferenceMessenger.Default.Register<HoldingUpdatedMessage>(this, Receive);

            HoldingRegistry = null;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var holdings) =
                await _apiService.GetAsync<ObservableCollection<HoldingInfo>>("Holding", "getHoldings", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _holdings.Clear();

            foreach (var holding in holdings)
                _holdings.Add(holding);
        }

        [RelayCommand]
        private void CloseModal()
        {
            HoldingRegistry = null;
        }

        [RelayCommand]
        private void OpenAddModal()
        {
            HoldingRegistry = new HoldingRegistryViewModel(_userStore, _apiService, CloseModalCommand);
        }

        [RelayCommand(CanExecute = nameof(IsHoldingSelected))]
        private void OpenUpdateModal()
        {
            HoldingRegistry = new HoldingRegistryViewModel(_userStore, _apiService, CloseModalCommand, SelectedHolding);
        }

        public void Receive(object recipient, HoldingUpdatedMessage message)
        {
            HoldingInfo holdingInfo = message.Value;

            if (IsHoldingSelected && SelectedHolding.EduYear == holdingInfo.EduYear)
            {
                SelectedHolding.StartDate = holdingInfo.StartDate;
                SelectedHolding.EndDate = holdingInfo.EndDate;
                SelectedHolding = null;
                return;
            }

            _holdings.Add(holdingInfo);
            SelectedHolding = null;
        }

        [RelayCommand(CanExecute = nameof(IsHoldingSelected))]
        private async Task DeleteHolding()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            if (SelectedHolding is null)
                return;

            (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>(
                        "Holding", $"deleteHolding/{SelectedHolding.EduYear}", _userStore.AccessToken);

            if (!HasErrorMessage)
            {
                _holdings.Remove(SelectedHolding);
                SelectedHolding = null;
            }

            IsWaiting = false;
        }
    }
}
