using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    public partial class HoldingRegistryViewModel : ViewModelBaseValidationExtended
    {
        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(HoldingRegistryViewModel),
            nameof(ValidateEduYear))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private int _eduYear;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(HoldingRegistryViewModel),
            nameof(ValidateStartDateBeforeEndDate))]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private DateTime _startDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private DateTime _endDate;

        public override bool CanSubmit => !HasErrors;

        partial void OnStartDateChanged(DateTime value)
        {
            ValidateProperty(EndDate, nameof(EndDate));
        }

        partial void OnEndDateChanged(DateTime value)
        {
            ValidateProperty(StartDate, nameof(StartDate));
        }

        public static ValidationResult ValidateEduYear(string name, ValidationContext context)
        {
            HoldingRegistryViewModel viewModel = (HoldingRegistryViewModel)context.ObjectInstance;

            if (viewModel.EduYear > 2019 && viewModel.EduYear < 2156)
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

        public HoldingRegistryViewModel(UserStore userStore, ApiService apiService,
            IRelayCommand closeCommand, HoldingInfo? holdingInfo = null) : base(apiService, userStore, closeCommand)
        {
            IsAddMode = holdingInfo is null;

            SubmitCommand = IsAddMode ? AddCommand : UpdateCommand;

            Header = IsAddMode ? "Додати навчальний рік" : "Редагувати навчальний рік";

            EduYear = holdingInfo?.EduYear ?? DateTime.Today.Year;
            StartDate = holdingInfo?.StartDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
            EndDate = holdingInfo?.EndDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now.AddDays(1);
        }

        protected override async Task Add()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var newHolding = InithializeInstance();

                (ErrorMessage, var addedHolding) =
                    await _apiService.PostAsync<HoldingInfo>("Holding", "addHolding", newHolding, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedHolding);
            });
        }

        protected override async Task Update()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            await ExecuteWithWaiting(async () =>
            {
                var updatedHolding = InithializeInstance();

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

        private HoldingInfo InithializeInstance()
        {
            return new HoldingInfo
            {
                EduYear = (short)EduYear,
                StartDate = DateOnly.FromDateTime(StartDate),
                EndDate = DateOnly.FromDateTime(EndDate)
            };
        }
    }
}
