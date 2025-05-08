using Client.Stores.NavigationStores;
using Client.ViewModels.Base.PageBase;
using Client.ViewModels.NavigationViewModel;

namespace Client.ViewModels
{
    public partial class SupAdminViewModel : PageViewModelBase
    {
        public SupAdminViewModel(SuccsefulLoginViewModel succsefulLoginViewModel,
            FrameNavigationStore frameNavigationStore, FrameNavigationViewModel frameNavigation) :
            base(succsefulLoginViewModel, frameNavigationStore)
        {
            _frameNavigationStore.CurrentFrameViewModelChanged += OnCurrentFrameViewModelChanged;
            ChangeFrame = frameNavigation.SupAdminNavigate;

            Task.Run(async () => await Navigate("Home"));
        }

        protected override void OnDeactivated()
        {
            _frameNavigationStore.CurrentFrameViewModelChanged -= OnCurrentFrameViewModelChanged;

            base.OnDeactivated();
        }
    }
}
