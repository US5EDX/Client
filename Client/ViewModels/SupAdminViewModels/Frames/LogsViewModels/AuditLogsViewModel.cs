using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class AuditLogsViewModel :
        PaginationFrameViewModelBase
    {
        public List<string> ActionTypes { get; init; }

        public ObservableCollection<AuditLogInfo> Logs { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanOpenLogDetails))]
        [NotifyCanExecuteChangedFor(nameof(OpenLogDetailsCommand))]
        private AuditLogInfo? _selectedLog;

        [ObservableProperty]
        private string? _selectedActionType;

        [ObservableProperty]
        private DateTime _leftBorder;

        [ObservableProperty]
        private DateTime _rightBorder;

        [ObservableProperty]
        private bool _hasDescription;

        public bool CanOpenLogDetails => SelectedLog is not null &&
            (SelectedLog.OldValue is not null || SelectedLog.NewValue is not null);

        public AuditLogsViewModel(ApiService apiService, UserStore userStore) : base(apiService, userStore)
        {
            ActionTypes = ["Added", "Modified", "Deleted"];
            Logs = [];

            _selectedActionType = ActionTypes[0];
            _leftBorder = DateTime.Today;
            _rightBorder = DateTime.Today;
            _hasDescription = false;

            Filter = FilterLogs;
        }

        public override async Task LoadContentAsync()
        {
            await UpdateListingAsync();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        async partial void OnSelectedActionTypeChanged(string? value) => await UpdateListingAsync();
        async partial void OnLeftBorderChanged(DateTime value) => await UpdateListingAsync();
        async partial void OnRightBorderChanged(DateTime value) => await UpdateListingAsync();
        async partial void OnHasDescriptionChanged(bool value) => await UpdateListingAsync();

        protected override async Task LoadDataAsync(int page)
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var logs) =
                await _apiService.GetAsync<List<AuditLogInfo>>("Audit",
                $"{page}/{PageSize}?actionType={SelectedActionType}" +
                $"&leftBorder={LeftBorder:yyyy-MM-dd}&rightBorder={RightBorder:yyyy-MM-dd}&hasDescription={HasDescription}",
                _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Logs.Clear();

                    foreach (var log in logs ?? [])
                        Logs.Add(log);

                    if (Logs.Count > 0)
                        CurrentPage = page;
                }
            });
        }

        private async Task UpdateListingAsync()
        {
            if (SelectedActionType is null)
            {
                ErrorMessage = "Необрано тип дії";
                return;
            }

            if (LeftBorder > RightBorder)
            {
                ErrorMessage = "Дата 'з' не може бути більшою за дату 'по'";
                return;
            }

            await ExecuteWithWaiting(async () => await LoadTotalPagesAsync("Audit",
                $"getCount?actionType={SelectedActionType}" +
                $"&leftBorder={LeftBorder:yyyy-MM-dd}&rightBorder={RightBorder:yyyy-MM-dd}&hasDescription={HasDescription}"));

            if (HasErrorMessage) return;

            await LoadDataAsync(1);
        }

        [RelayCommand(CanExecute = nameof(CanOpenLogDetails))]
        private void OpenLogDetails() => SelectedModal = new LogDetailsViewModel(SelectedLog!, CloseModalCommand);

        private bool FilterLogs(object log, string filter)
        {
            if (log is not AuditLogInfo logInfo) return false;

            return logInfo.UserId.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                logInfo.EntityName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                (logInfo.IpAddress is not null && logInfo.IpAddress.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                (logInfo.EntityId is not null && logInfo.EntityId.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                (logInfo.Description is not null && logInfo.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
    }
}
