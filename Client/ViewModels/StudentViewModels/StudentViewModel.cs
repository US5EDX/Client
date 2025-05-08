using Client.Stores;
using Client.Stores.NavigationStores;
using Client.ViewModels.Base.PageBase;
using Client.ViewModels.NavigationViewModel;

namespace Client.ViewModels
{
    public partial class StudentViewModel : PageViewModelBase
    {
        private readonly UserStore _userStore;

        public bool IsHeadmen => _userStore.StudentInfo.Headman;

        public StudentViewModel(SuccsefulLoginViewModel succsefulLoginViewModel,
            FrameNavigationStore frameNavigationStore, FrameNavigationViewModel frameNavigation,
            UserStore userStore) : base(succsefulLoginViewModel, frameNavigationStore)
        {
            _frameNavigationStore.CurrentFrameViewModelChanged += OnCurrentFrameViewModelChanged;
            ChangeFrame = frameNavigation.StudentNavigate;

            _userStore = userStore;

            Task.Run(async () => await Navigate("Home"));
        }

        protected override void OnDeactivated()
        {
            _frameNavigationStore.CurrentFrameViewModelChanged -= OnCurrentFrameViewModelChanged;

            base.OnDeactivated();
        }
    }
}
