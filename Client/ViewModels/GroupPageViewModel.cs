using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels
{
    public partial class GroupPageViewModel : ObservableRecipient, IFrameViewModel
    {
        public async Task LoadContentAsync()
        {
            await Task.Delay(2000);

            throw new NotImplementedException();
        }
    }
}
