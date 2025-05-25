using Client.Models;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base
{
    public abstract partial class SettingViewModel<T>(ApiService apiService, UserStore userStore, IMessageService messageService) :
        ViewModelBaseWithValidation(apiService, userStore), IFrameViewModel
    {
        protected string Key { get; init; } = null!;

        public abstract bool CanSubmit { get; }

        protected abstract void SetProperties(T value);

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var thresholds) =
                await _apiService.GetAsync<T>
                ("Setting", Key, _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            SetProperties(thresholds!);
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task SubmitChanges()
        {
            ValidateAllProperties();

            if (HasErrors) return;

            var setting = InithializeInstance()!;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<object?>("Setting", Key, setting, _userStore.AccessToken);
            });

            if (!HasErrorMessage)
                messageService.ShowInfoMessage("Зміни успішно внесено");
        }

        protected abstract T InithializeInstance();
    }
}
