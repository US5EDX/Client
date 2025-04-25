using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    [ObservableRecipient]
    public partial class RecordRegistryViewModel : ObservableValidator, IPageViewModel
    {
        private CancellationTokenSource _cts;

        private readonly ApiService _apiService;
        private readonly UserStore _userStore;

        public List<SemesterInfo> Semesters { get; init; }
        public ObservableCollection<DisciplineShortInfo> Disciplines { get; init; }

        public string Header { get; init; }

        private readonly uint? _recordId;
        private readonly string _studentId;
        private readonly short _holding;
        private readonly byte _eduLevel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddRecordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateRecordCommand))]
        private SemesterInfo? _semester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddRecordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateRecordCommand))]
        private DisciplineShortInfo? _discipline;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        private string _disciplineCodeName;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool IsAddMode { get; init; }

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanSubmit => Discipline is not null && Semester is not null;

        public IRelayCommand CloseCommand { get; init; }

        public IAsyncRelayCommand SubmitCommand { get; init; }

        public RecordRegistryViewModel(ApiService apiService, UserStore userStore, IRelayCommand closeCommand, byte eduLevel,
            string studentId, short holding, RecordInfo? record = null)
        {
            _apiService = apiService;
            _userStore = userStore;

            CloseCommand = closeCommand;
            SubmitCommand = record is null ? AddRecordCommand : UpdateRecordCommand;

            Header = record is null ? "Додати запис" : "Редагувати запис";
            IsAddMode = record is null;

            Semesters = new List<SemesterInfo>(2);

            if (record is null || record.ChosenSemester == 1)
                Semesters.Add(new SemesterInfo() { SemesterId = 1, SemesterName = "Непарний" });

            if (record is null || record.ChosenSemester == 2)
                Semesters.Add(new SemesterInfo() { SemesterId = 2, SemesterName = "Парний" });

            _semester = Semesters[0];

            Disciplines = new ObservableCollection<DisciplineShortInfo>();

            if (record is not null)
            {
                Disciplines.Add(new DisciplineShortInfo()
                { DisciplineId = record.DisciplineId, DisciplineCodeName = $"{record.DisciplineCode} {record.DisciplineName}" });

                _discipline = Disciplines[0];
                _disciplineCodeName = _discipline.DisciplineCodeName;
            }

            _recordId = record?.RecordId;
            _studentId = studentId;
            _holding = holding;
            _eduLevel = eduLevel;
        }

        partial void OnDisciplineCodeNameChanged(string value)
        {
            if (Discipline is not null && Discipline.DisciplineCodeName == value)
                return;

            if (value.Length < 3)
            {
                Disciplines.Clear();
                Discipline = null;
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                await Task.Delay(500);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Disciplines.Clear();
                    Disciplines.Add(new DisciplineShortInfo { DisciplineCodeName = "Пошук..." });
                    IsLoading = true;
                });

                var result = await LoadDisciplinesFromServer(value);

                if (token.IsCancellationRequested)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Disciplines.Clear();
                    foreach (var item in result)
                        Disciplines.Add(item);
                    IsLoading = false;
                });
            }, token);
        }

        partial void OnSemesterChanged(SemesterInfo? value)
        {
            DisciplineCodeName = string.Empty;
        }

        private async Task<IEnumerable<DisciplineShortInfo>> LoadDisciplinesFromServer(string searchFilter)
        {
            if (searchFilter.Length < 3 || searchFilter.Length > 50)
                return Enumerable.Empty<DisciplineShortInfo>();

            (ErrorMessage, var disciplines) =
                await _apiService.GetAsync<ObservableCollection<DisciplineShortInfo>>("Discipline",
                $"getShortInfo?holding={_holding}&eduLevel={_eduLevel}&semester={Semester.SemesterId}&code={searchFilter}",
                _userStore.AccessToken);

            return disciplines ?? Enumerable.Empty<DisciplineShortInfo>();
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task AddRecord()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newRecord = InitializeRecord();

                (ErrorMessage, var addedRecord) =
                    await _apiService.PostAsync<RecordInfo>("Record", "addRecord", newRecord, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedRecord);
            });
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task UpdateRecord()
        {
            await ExecuteWithWaiting(async () =>
            {
                var recordToUpdate = InitializeRecord();

                (ErrorMessage, var updatedRecord) =
                    await _apiService.PutAsync<RecordInfo>("Record", "updateRecord", recordToUpdate, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(updatedRecord);
            });
        }

        private void OnSubmitAccepted(RecordInfo recordInfo)
        {
            WeakReferenceMessenger.Default.Send(new RecordUpdatedMessage(recordInfo));
            CloseCommand.Execute(null);
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private RecordRegistryInfo InitializeRecord()
        {
            return new RecordRegistryInfo
            {
                RecordId = _recordId,
                StudentId = _studentId,
                DisciplineId = Discipline.DisciplineId,
                Semester = Semester.SemesterId,
                Holding = _holding
            };
        }
    }
}
