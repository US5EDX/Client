using Client.Models;
using Client.PdfDoucments;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace Client.ViewModels
{
    public partial class SignedStudentsPageViewModel : FrameBaseViewModel
    {
        private readonly DisciplineMainInfoStore _disciplineStore;
        private readonly IMessageService _messageService;

        public ObservableCollection<RecordWithStudentInfo> Records { get; init; }
        public List<SemesterInfo> SemesterInfos { get; init; }

        [ObservableProperty]
        private SemesterInfo? _selectedSemester;

        public string Header { get; init; }

        public int Total => Records.Count;

        public IRelayCommand CloseCommand { get; init; }

        public Func<object, string, bool> Filter { get; init; }

        public SignedStudentsPageViewModel(ApiService apiService, UserStore userStore, DisciplineMainInfoStore disciplineStore,
            IMessageService messageService, IRelayCommand closeCommand) : base(apiService, userStore)
        {
            _disciplineStore = disciplineStore;
            _messageService = messageService;

            Header = $"{_disciplineStore.DisciplineCode} {_disciplineStore.DisciplineName}";

            Records = [];
            SemesterInfos = [];

            if (_disciplineStore.Semester == 0 || _disciplineStore.Semester == 1)
                SemesterInfos.Add(new SemesterInfo() { SemesterId = 1, SemesterName = "Непарний" });

            if (_disciplineStore.Semester == 0 || _disciplineStore.Semester == 2)
                SemesterInfos.Add(new SemesterInfo() { SemesterId = 2, SemesterName = "Парний" });

            if (SemesterInfos.Count == 0)
                throw new InvalidDataException("Неправильно передані дані");

            _selectedSemester = SemesterInfos[0];

            CloseCommand = closeCommand;
            Filter = FilterStudents;
        }

        public override async Task LoadContentAsync()
        {
            await UpdateRecords();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        async partial void OnSelectedSemesterChanged(SemesterInfo? value) => await ExecuteWithWaiting(UpdateRecords);

        private async Task UpdateRecords()
        {
            (ErrorMessage, var records) =
                await _apiService.GetAsync<ObservableCollection<RecordWithStudentInfo>>("Record",
                $"getSignedStudents?disciplineId={_disciplineStore.DisciplineId}&semester={SelectedSemester.SemesterId}",
                _userStore.AccessToken);

            if (!HasErrorMessage)
            {
                Records.Clear();

                foreach (var record in records ?? Enumerable.Empty<RecordWithStudentInfo>())
                    Records.Add(record);

                OnPropertyChanged(nameof(Total));
            }
        }

        [RelayCommand]
        private async Task GeneratePdf()
        {
            var path = _messageService.ShowSaveFileDialog("Вибіріть місце збереження", "Pdf file|*.pdf");

            if (path is null) return;

            var reportDocument = new SignedStudentsReportDocument(Records, Header, SelectedSemester.SemesterName, Total);

            await ExecuteWithWaiting(async () => ErrorMessage = await PdfGenerator.GeneratePdf(reportDocument, path));
        }

        private bool FilterStudents(object record, string filter)
        {
            if (record is not RecordWithStudentInfo recordInfo) return false;

            return recordInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.FacultyName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.GroupCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
