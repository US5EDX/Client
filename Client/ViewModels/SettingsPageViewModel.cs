using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace Client.ViewModels
{
    public partial class SettingsPageViewModel(ApiService apiService, UserStore userStore, IMessageService messageService) :
        ViewModelBaseWithValidation(apiService, userStore), IFrameViewModel
    {
        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _bachelorNotEnough;

        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _bachelorPartiallyFilled;

        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _masterNotEnough;

        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _masterPartiallyFilled;

        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _phDNotEnough;

        [ObservableProperty]
        [Required]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _phDPartiallyFilled;

        public bool CanSubmit => !HasErrors;

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var thresholds) =
                await _apiService.GetAsync<DisciplineStatusThresholds>
                ("Discipline", $"getTresholds", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            BachelorNotEnough = thresholds.Bachelor.NotEnough;
            BachelorPartiallyFilled = thresholds.Bachelor.PartiallyFilled;
            MasterNotEnough = thresholds.Master.NotEnough;
            MasterPartiallyFilled = thresholds.Master.PartiallyFilled;
            PhDNotEnough = thresholds.PhD.NotEnough;
            PhDPartiallyFilled = thresholds.PhD.PartiallyFilled;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task SubmitChanges()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            var thresholds = new DisciplineStatusThresholds
            {
                Bachelor = new ThresholdValue { NotEnough = BachelorNotEnough, PartiallyFilled = BachelorPartiallyFilled },
                Master = new ThresholdValue { NotEnough = MasterNotEnough, PartiallyFilled = MasterPartiallyFilled },
                PhD = new ThresholdValue { NotEnough = PhDNotEnough, PartiallyFilled = PhDPartiallyFilled },
            };

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object?>("Discipline", "updateTresholds", thresholds, _userStore.AccessToken);

                if (!HasErrorMessage)
                    messageService.ShowInfoMessage("Зміни успішно внесено");
            });
        }
    }
}
