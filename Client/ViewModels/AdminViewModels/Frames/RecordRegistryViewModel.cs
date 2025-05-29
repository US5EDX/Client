using Client.Models;
using Client.Services;
using Client.Stores;
using Client.Stores.Messangers;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class RecordRegistryViewModel : ViewModelBaseValidationExtended
    {
        private CancellationTokenSource _cts = null!;

        public List<SemesterInfo> Semesters { get; init; }
        public ObservableCollection<DisciplineShortInfo> Disciplines { get; init; }

        private readonly uint? _recordId;
        private readonly string _studentId;
        private readonly short _holding;
        private readonly byte _eduLevel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private SemesterInfo? _semester;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSubmit))]
        [NotifyCanExecuteChangedFor(nameof(AddCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
        private DisciplineShortInfo? _discipline;

        [ObservableProperty]
        private string? _disciplineCodeName;

        [ObservableProperty]
        private bool _isLoading;

        public override bool CanSubmit => Discipline is not null && Semester is not null;

        public RecordRegistryViewModel(ApiService apiService, UserStore userStore, IRelayCommand closeCommand, byte eduLevel,
            string studentId, short holding, RecordInfo? record = null) : base(apiService, userStore, closeCommand)
        {
            IsAddMode = record is null;

            SubmitCommand = IsAddMode ? AddCommand : UpdateCommand;

            Header = IsAddMode ? "Додати запис" : "Редагувати запис";

            Semesters = new List<SemesterInfo>(2);

            if (IsAddMode || record.ChosenSemester == 1)
                Semesters.Add(new() { SemesterId = 1, SemesterName = "Осінній" });

            if (IsAddMode || record.ChosenSemester == 2)
                Semesters.Add(new() { SemesterId = 2, SemesterName = "Весняний" });

            _semester = Semesters[0];

            Disciplines = [];

            if (!IsAddMode)
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

        partial void OnSemesterChanged(SemesterInfo? value) => DisciplineCodeName = string.Empty;

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

                var result = await LoadDisciplinesAsync(value);

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

        private async Task<IEnumerable<DisciplineShortInfo>> LoadDisciplinesAsync(string searchFilter)
        {
            if (searchFilter.Length < 3 || searchFilter.Length > 50)
                return [];

            (ErrorMessage, var disciplines) =
                await _apiService.GetAsync<ObservableCollection<DisciplineShortInfo>>("Discipline",
                $"getShortInfo?holding={_holding}&eduLevel={_eduLevel}&semester={Semester.SemesterId}&code={searchFilter}",
                _userStore.AccessToken);

            return disciplines ?? Enumerable.Empty<DisciplineShortInfo>();
        }

        protected override async Task Add()
        {
            await ExecuteWithWaiting(async () =>
            {
                var newRecord = InitializeInstance();

                (ErrorMessage, var addedRecord) =
                    await _apiService.PostAsync<RecordInfo>("Record", "addRecord", newRecord, _userStore.AccessToken);

                if (!HasErrorMessage)
                    OnSubmitAccepted(addedRecord);
            });
        }

        protected override async Task Update()
        {
            await ExecuteWithWaiting(async () =>
            {
                var recordToUpdate = InitializeInstance();

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

        private RecordRegistryInfo InitializeInstance()
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
