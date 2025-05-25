using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels
{
    public partial class SettingsPageViewModel : ObservableRecipient, IFrameViewModel
    {
        public record SettingsTabRecord(string Title, string Icon, IFrameViewModel Content);

        public List<SettingsTabRecord> SettingTabs { get; init; }

        [ObservableProperty]
        private SettingsTabRecord _selectedTab;

        public SettingsPageViewModel(ThresholdsViewModel thresholdsViewModel)
        {
            SettingTabs = [new SettingsTabRecord("Пороги набраності", "DoorOpen", thresholdsViewModel)];
            _selectedTab = SettingTabs[0];
        }

        public async Task LoadContentAsync()
        {
            foreach (var setting in SettingTabs)
                await setting.Content.LoadContentAsync();
        }
    }
}
