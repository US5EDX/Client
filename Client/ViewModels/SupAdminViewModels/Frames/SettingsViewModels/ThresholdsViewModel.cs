using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Client.ViewModels
{
    public partial class ThresholdsViewModel : SettingViewModel<DisciplineStatusThresholds>
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
        [NotNull]
        [Range(0, 100)]
        [NotifyDataErrorInfo]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(SubmitChangesCommand))]
        private int _phDPartiallyFilled;

        public ThresholdsViewModel(ApiService apiService, UserStore userStore, IMessageService messageService) :
        base(apiService, userStore, messageService) => Key = "Thresholds";

        public override bool CanSubmit => !HasErrors;

        protected override void SetProperties(DisciplineStatusThresholds value)
        {
            BachelorNotEnough = value.Bachelor.NotEnough;
            BachelorPartiallyFilled = value.Bachelor.PartiallyFilled;
            MasterNotEnough = value.Master.NotEnough;
            MasterPartiallyFilled = value.Master.PartiallyFilled;
            PhDNotEnough = value.PhD.NotEnough;
            PhDPartiallyFilled = value.PhD.PartiallyFilled;
        }

        protected override DisciplineStatusThresholds InithializeInstance() =>
            new()
            {
                Bachelor = new ThresholdValue { NotEnough = BachelorNotEnough, PartiallyFilled = BachelorPartiallyFilled },
                Master = new ThresholdValue { NotEnough = MasterNotEnough, PartiallyFilled = MasterPartiallyFilled },
                PhD = new ThresholdValue { NotEnough = PhDNotEnough, PartiallyFilled = PhDPartiallyFilled },
            };
    }
}
