using Client.Services;
using Client.Stores.NavigationStores;
using Client.ViewModels.Base.PageBase;
using Client.ViewModels.NavigationViewModel;

namespace Client.ViewModels
{
    public partial class AdminViewModel : PageViewModelBase
    {
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentCohicesNavigationService;
        private readonly FrameNavigationService<StudentYearChoicesViewModel> _studentYearChoicesNavigationService;
        private readonly FrameNavigationService<DisciplinesPageViewModel> _disciplinesPageNavigationService;

        public AdminViewModel(SuccsefulLoginViewModel succsefulLoginViewModel,
        FrameNavigationStore frameNavigationStore, FrameNavigationViewModel frameNavigation,
            FrameNavigationService<GroupPageViewModel> groupNavigationService,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentCohicesNavigationService,
            FrameNavigationService<StudentYearChoicesViewModel> studentYearChoicesNavigationService,
            FrameNavigationService<DisciplinesPageViewModel> disciplinesPageNavigationService) :
            base(succsefulLoginViewModel, frameNavigationStore)
        {
            _frameNavigationStore.CurrentFrameViewModelChanged += OnCurrentFrameViewModelChanged;
            ChangeFrame = frameNavigation.AdminNavigate;

            _allStudentCohicesNavigationService = allStudentCohicesNavigationService;
            _studentYearChoicesNavigationService = studentYearChoicesNavigationService;
            _groupNavigationService = groupNavigationService;
            _disciplinesPageNavigationService = disciplinesPageNavigationService;

            _groupNavigationService.OnNavigationRequested += Navigate;
            _allStudentCohicesNavigationService.OnNavigationRequested += Navigate;
            _studentYearChoicesNavigationService.OnNavigationRequested += Navigate;
            _disciplinesPageNavigationService.OnNavigationRequested += Navigate;

            Task.Run(async () => await Navigate("Home"));
        }

        protected override void OnDeactivated()
        {
            _frameNavigationStore.CurrentFrameViewModelChanged -= OnCurrentFrameViewModelChanged;
            _groupNavigationService.OnNavigationRequested -= Navigate;
            _allStudentCohicesNavigationService.OnNavigationRequested -= Navigate;
            _studentYearChoicesNavigationService.OnNavigationRequested -= Navigate;
            _disciplinesPageNavigationService.OnNavigationRequested -= Navigate;

            base.OnDeactivated();
        }
    }
}
