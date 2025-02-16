using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class HoldingRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private readonly UserStore _userStore;
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string _header;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(HoldingRegistryViewModel),
            nameof(ValidateEduYear))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddHoldingCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateHoldingCommand))]
        private DateTime _eduYear;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(HoldingRegistryViewModel),
            nameof(ValidateStartDateBeforeEndDate))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddHoldingCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateHoldingCommand))]
        private DateTime _startDate;

        partial void OnStartDateChanged(DateTime value)
        {
            ValidateProperty(EndDate, nameof(EndDate));
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddHoldingCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateHoldingCommand))]
        private DateTime _endDate;

        partial void OnEndDateChanged(DateTime value)
        {
            ValidateProperty(StartDate, nameof(StartDate));
        }

        public static ValidationResult ValidateEduYear(string name, ValidationContext context)
        {
            HoldingRegistryViewModel viewModel = (HoldingRegistryViewModel)context.ObjectInstance;

            if (viewModel.EduYear.Year > 2019 && viewModel.EduYear.Year < 2156)
                return ValidationResult.Success;

            return new("Навчальний рік повинен бути у межах 2020 - 2155");
        }

        public static ValidationResult ValidateStartDateBeforeEndDate(string name, ValidationContext context)
        {
            HoldingRegistryViewModel viewModel = (HoldingRegistryViewModel)context.ObjectInstance;

            if (viewModel.StartDate.Date < viewModel.EndDate.Date)
                return ValidationResult.Success;

            return new("Дата початку не може бути пізніше дати закінчення");
        }

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        private bool _isAddMode;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => !HasErrors;

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public HoldingRegistryViewModel(UserStore userStore, ApiService apiService,
            IRelayCommand closeCommand, HoldingInfo? holdingInfo = null)
        {
            _userStore = userStore;
            _apiService = apiService;

            CloseCommand = closeCommand;
            SubmitCommand = holdingInfo is null ? AddHoldingCommand : UpdateHoldingCommand;

            Header = holdingInfo is null ? "Додати навчальний рік" : "Редагувати навчальний рік";
            IsAddMode = holdingInfo is null;

            EduYear = new DateTime(holdingInfo?.EduYear ?? DateTime.Now.Year, 1, 1);
            StartDate = holdingInfo?.StartDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
            EndDate = holdingInfo?.EndDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now.AddDays(1);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddHolding()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newHolding = new HoldingInfo
                {
                    EduYear = (short)EduYear.Year,
                    StartDate = DateOnly.FromDateTime(StartDate),
                    EndDate = DateOnly.FromDateTime(EndDate)
                };

                (ErrorMessage, var addedHolding) =
                    await _apiService.PostAsync<HoldingInfo>("Holding", "addHolding", newHolding, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedHolding);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateHolding()
        {
            await ExecuteWithWaiting(async () =>
            {
                var updatedHolding = new HoldingInfo
                {
                    EduYear = (short)EduYear.Year,
                    StartDate = DateOnly.FromDateTime(StartDate),
                    EndDate = DateOnly.FromDateTime(EndDate)
                };

                (ErrorMessage, _) =
                    await _apiService.PutAsync<HoldingInfo>("Holding", "updateHolding", updatedHolding, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedHolding);
            });
        }

        private void OnSubmitAccepted(HoldingInfo holdingInfo)
        {
            WeakReferenceMessenger.Default.Send(new HoldingUpdatedMessage(holdingInfo));
            CloseCommand.Execute(null);
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }
    }
}
