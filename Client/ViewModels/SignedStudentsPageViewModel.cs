using Client.Models;
using Client.PdfDoucments;
using Client.Services;
using Client.Services.MessageService;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;

namespace Client.ViewModels
{
    public partial class SignedStudentsPageViewModel : ObservableRecipient, IPageViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly DisciplineMainInfoStore _disciplineStore;
        private readonly IMessageService _messageService;

        private readonly ObservableCollection<RecordWithStudentInfo> _records;
        private readonly List<SemesterInfo> _semesterInfos;

        public IEnumerable<RecordWithStudentInfo> Records => _records;
        public IEnumerable<SemesterInfo> SemesterInfos => _semesterInfos;

        [ObservableProperty]
        private SemesterInfo? _selectedSemester;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public string Header { get; init; }

        public int Total => _records.Count;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public IRelayCommand CloseCommand { get; init; }

        public Func<object, string, bool> Filter { get; init; }

        public SignedStudentsPageViewModel(ApiService apiService, UserStore userStore, DisciplineMainInfoStore disciplineStore,
            IMessageService messageService, IRelayCommand closeCommand)
        {
            _apiService = apiService;
            _userStore = userStore;
            _disciplineStore = disciplineStore;
            _messageService = messageService;

            Header = $"{_disciplineStore.DisciplineCode} {_disciplineStore.DisciplineName}";

            _records = new ObservableCollection<RecordWithStudentInfo>();

            _semesterInfos = new List<SemesterInfo>();

            if (_disciplineStore.Semester == 0 || _disciplineStore.Semester == 1)
                _semesterInfos.Add(new SemesterInfo() { SemesterId = 1, SemesterName = "Непарний" });

            if (_disciplineStore.Semester == 0 || _disciplineStore.Semester == 2)
                _semesterInfos.Add(new SemesterInfo() { SemesterId = 2, SemesterName = "Парний" });

            if (_semesterInfos.Count == 0)
                throw new InvalidDataException("Неправильно передані дані");

            _selectedSemester = _semesterInfos[0];

            CloseCommand = closeCommand;
            Filter = FilterStudents;
        }

        public async Task LoadContentAsync()
        {
            await UpdateRecords();

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);
        }

        partial void OnSelectedSemesterChanged(SemesterInfo? value)
        {
            UpdateRecords().ConfigureAwait(false);
        }

        private async Task UpdateRecords()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            (ErrorMessage, var records) =
                await _apiService.GetAsync<ObservableCollection<RecordWithStudentInfo>>("Record",
                $"getSignedStudents?disciplineId={_disciplineStore.DisciplineId}&semester={SelectedSemester.SemesterId}",
                _userStore.AccessToken);

            if (!HasErrorMessage)
            {
                _records.Clear();

                foreach (var record in records ?? Enumerable.Empty<RecordWithStudentInfo>())
                    _records.Add(record);

                OnPropertyChanged(nameof(Total));
            }

            IsWaiting = false;
        }

        [RelayCommand]
        private async Task GeneratePdf()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            var path = _messageService.ShowSaveFileDialog("Вибіріть місце збереження", "Pdf file|*.pdf");

            if (path is null)
            {
                IsWaiting = false;
                return;
            }

            var reportDocument = new SignedStudentsReportDocument(_records, Header, SelectedSemester.SemesterName, Total);
            ErrorMessage = await PdfGenerator.GeneratePdf(reportDocument, path);

            IsWaiting = false;
        }

        private bool FilterStudents(object record, string filter)
        {
            if (record is not RecordWithStudentInfo recordInfo)
                return false;

            return recordInfo.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.Email.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.FacultyName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                recordInfo.GroupCode.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
