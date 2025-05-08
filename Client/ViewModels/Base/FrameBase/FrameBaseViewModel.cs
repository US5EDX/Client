using Client.Services;
using Client.Stores;

namespace Client.ViewModels.Base
{
    public abstract partial class FrameBaseViewModel(ApiService apiService, UserStore userStore) :
        ViewModelBase(apiService, userStore), IFrameViewModel
    {
        public abstract Task LoadContentAsync();
    }
}