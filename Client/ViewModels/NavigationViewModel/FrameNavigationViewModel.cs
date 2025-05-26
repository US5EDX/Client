using Client.Services;
using Client.Stores.NavigationStores;

namespace Client.ViewModels.NavigationViewModel
{
    public class FrameNavigationViewModel
    {
        private readonly FrameNavigationService<HomePageViewModel> _homeNavigationService;
        private readonly FrameNavigationService<FacultiesPageViewModel> _facultiesNavigationService;
        private readonly FrameNavigationService<HoldingPageViewModel> _holdingNavigationService;
        private readonly FrameNavigationService<WorkersPageViewModel> _usersNavigationService;
        private readonly FrameNavigationService<SpecialtiesPageViewModel> _specialtiesNavigationService;
        private readonly FrameNavigationService<AcademiciansPageViewModel> _accademiciansNavigationService;
        private readonly FrameNavigationService<GroupsPageViewModel> _groupsNavigationService;
        private readonly FrameNavigationService<GroupPageViewModel> _groupNavigationService;
        private readonly FrameNavigationService<DisciplinesPageViewModel> _disciplinesNavigationService;
        private readonly FrameNavigationService<AllStudentChoicesViewModel> _allStudentChoicesNavigationService;
        private readonly FrameNavigationService<StudentYearChoicesViewModel> _studentYearChoicesNavigationService;
        private readonly FrameNavigationService<StudentChoosingPageViewModel> _studentChoosingNavigationService;
        private readonly FrameNavigationService<DisciplinesForStudentViewModel> _disciplinesForStudentNavigationService;
        private readonly FrameNavigationService<StudentChoicesViewModel> _studentChoicesNavigationService;
        private readonly FrameNavigationService<SettingsPageViewModel> _settingsNavigationService;
        private readonly FrameNavigationService<AuditLogsViewModel> _auditLogsNavigationService;

        public FrameNavigationViewModel(
            FrameNavigationStore frameNavigationStore,
            FrameNavigationService<HomePageViewModel> homeNavigationService,
            FrameNavigationService<FacultiesPageViewModel> facultiesNavigationService,
            FrameNavigationService<HoldingPageViewModel> holdingNavigationService,
            FrameNavigationService<WorkersPageViewModel> usersNavigationService,
            FrameNavigationService<SpecialtiesPageViewModel> specialtiesNavigationService,
            FrameNavigationService<AcademiciansPageViewModel> accademiciansNavigationService,
            FrameNavigationService<GroupsPageViewModel> groupsNavigationService,
            FrameNavigationService<GroupPageViewModel> groupNavigationService,
            FrameNavigationService<DisciplinesPageViewModel> disciplinesNavigationService,
            FrameNavigationService<AllStudentChoicesViewModel> allStudentChoicesNavigationService,
            FrameNavigationService<StudentYearChoicesViewModel> studentYearChoicesNavigationService,
            FrameNavigationService<StudentChoosingPageViewModel> studentChoosingNavigationService,
            FrameNavigationService<DisciplinesForStudentViewModel> disciplinesForStudentNavigationService,
            FrameNavigationService<StudentChoicesViewModel> studentChoicesNavigationService,
            FrameNavigationService<SettingsPageViewModel> settingsNavigationService,
            FrameNavigationService<AuditLogsViewModel> auditLogsNavigationService
            )
        {
            _homeNavigationService = homeNavigationService;
            _facultiesNavigationService = facultiesNavigationService;
            _holdingNavigationService = holdingNavigationService;
            _usersNavigationService = usersNavigationService;
            _specialtiesNavigationService = specialtiesNavigationService;
            _accademiciansNavigationService = accademiciansNavigationService;
            _groupsNavigationService = groupsNavigationService;
            _groupNavigationService = groupNavigationService;
            _disciplinesNavigationService = disciplinesNavigationService;
            _allStudentChoicesNavigationService = allStudentChoicesNavigationService;
            _studentYearChoicesNavigationService = studentYearChoicesNavigationService;
            _studentChoosingNavigationService = studentChoosingNavigationService;
            _disciplinesForStudentNavigationService = disciplinesForStudentNavigationService;
            _studentChoicesNavigationService = studentChoicesNavigationService;
            _settingsNavigationService = settingsNavigationService;
            _auditLogsNavigationService = auditLogsNavigationService;
        }

        public async Task SupAdminNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Users":
                    await _usersNavigationService.NavigateAsync();
                    break;
                case "Faculties":
                    await _facultiesNavigationService.NavigateAsync();
                    break;
                case "Holding":
                    await _holdingNavigationService.NavigateAsync();
                    break;
                case "Logs":
                    await _auditLogsNavigationService.NavigateAsync();
                    break;
                case "Settings":
                    await _settingsNavigationService.NavigateAsync();
                    break;
                default:
                    break;
            }
        }

        public async Task AdminNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Academicians":
                    await _accademiciansNavigationService.NavigateAsync();
                    break;
                case "Specialties":
                    await _specialtiesNavigationService.NavigateAsync();
                    break;
                case "Disciplines":
                    await _disciplinesNavigationService.NavigateAsync();
                    break;
                case "Groups":
                    await _groupsNavigationService.NavigateAsync();
                    break;
                case "Group":
                    await _groupNavigationService.NavigateAsync();
                    break;
                case "AllChoices":
                    await _allStudentChoicesNavigationService.NavigateAsync();
                    break;
                case "YearChoices":
                    await _studentYearChoicesNavigationService.NavigateAsync();
                    break;
                default:
                    break;
            }
        }

        public async Task LecturerNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Disciplines":
                    await _disciplinesNavigationService.NavigateAsync();
                    break;
                case "Groups":
                    await _groupsNavigationService.NavigateAsync();
                    break;
                case "Group":
                    await _groupNavigationService.NavigateAsync();
                    break;
                case "AllChoices":
                    await _allStudentChoicesNavigationService.NavigateAsync();
                    break;
                case "YearChoices":
                    await _studentYearChoicesNavigationService.NavigateAsync();
                    break;
                default:
                    break;
            }
        }

        public async Task StudentNavigate(string destination)
        {
            switch (destination)
            {
                case "Home":
                    _homeNavigationService.Navigate();
                    break;
                case "Disciplines":
                    await _disciplinesForStudentNavigationService.NavigateAsync();
                    break;
                case "Choices":
                    await _studentChoosingNavigationService.NavigateAsync();
                    break;
                case "MyChoices":
                    await _studentChoicesNavigationService.NavigateAsync();
                    break;
                case "Group":
                    await _groupNavigationService.NavigateAsync();
                    break;
                default:
                    break;
            }
        }
    }
}
