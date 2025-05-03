using Client.Models;
using Client.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Client.ViewModels.CustomControlsViewModels
{
    public partial class DisciplineComboBoxViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly string _accessToken;
        private readonly short _holding;
        private readonly byte _course;
        private readonly byte _semester;
        private readonly byte _eduLevel;

        private CancellationTokenSource _cts = null!;

        private uint? _recordId;
        private uint _oldDisciplineId;

        public uint OldDisciplineId => _oldDisciplineId;

        public string Label { get; }

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private DisciplineShortInfo? _selectedDiscipline;

        [ObservableProperty]
        private bool _status;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isSubmitting;

        [ObservableProperty]
        private string _faIcon = null!;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        [ObservableProperty]
        public bool _isEnabled;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public ObservableCollection<DisciplineShortInfo> Disciplines { get; init; }

        public DisciplineComboBoxViewModel(ApiService apiService, string accessToken, short holding, byte course, byte semester,
            byte eduLevel, string label, RecordShortInfo? recordShortInfo = null)
        {
            _apiService = apiService;
            _accessToken = accessToken;
            _holding = holding;
            _course = course;
            _semester = semester;
            _eduLevel = eduLevel;
            Label = label;
            Disciplines = new ObservableCollection<DisciplineShortInfo>();

            _recordId = recordShortInfo?.RecordId;
            _oldDisciplineId = recordShortInfo?.DisciplineId ?? 0;

            _isEnabled = true;

            if (recordShortInfo is null)
            {
                SearchText = string.Empty;
                return;
            }

            var disciplineCodeName = $"{recordShortInfo.DisciplineCode} {recordShortInfo.DisciplineName}";

            Disciplines.Add(new DisciplineShortInfo()
            {
                DisciplineId = recordShortInfo.DisciplineId,
                DisciplineCodeName = disciplineCodeName,
                IsYearLong = recordShortInfo.IsYearLong,
            });

            _selectedDiscipline = Disciplines.First();
            _searchText = disciplineCodeName;
            _status = recordShortInfo.Approved;

            _isEnabled = !_status;
        }

        public bool IsDisciplineChanged()
        {
            if (SelectedDiscipline is null || _oldDisciplineId == SelectedDiscipline.DisciplineId)
                return false;

            return true;
        }

        public async Task<bool> SubmitAsync()
        {
            if (!IsDisciplineChanged() || Status == true)
                return false;

            ErrorMessage = string.Empty;

            FaIcon = "Gear";
            IsSubmitting = true;
            IsEnabled = false;

            var record = new RecordRegistryWithoutStudent
            {
                RecordId = _recordId,
                DisciplineId = SelectedDiscipline.DisciplineId,
                Holding = _holding,
                Semester = _semester,
            };

            (ErrorMessage, uint recordId) =
                    await _apiService.PostAsync<uint>
                    ("Record", "registerRecord", record, _accessToken);

            if (!HasErrorMessage)
            {
                _recordId = recordId;
                _oldDisciplineId = SelectedDiscipline.DisciplineId;
            }

            IsSubmitting = false;
            IsEnabled = true;

            if (HasErrorMessage)
            {
                FaIcon = "ExclamationTriangle";
                return false;
            }

            FaIcon = "CheckCircle";

            return true;
        }

        partial void OnSearchTextChanged(string value)
        {
            if (SelectedDiscipline is not null && SelectedDiscipline.DisciplineCodeName == value)
                return;

            if (value.Length < 3)
            {
                _cts?.Cancel();
                Disciplines.Clear();
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

                var result = await LoadWorkersFromDatabase(value);

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

        private async Task<List<DisciplineShortInfo>> LoadWorkersFromDatabase(string searchText)
        {
            if (searchText.Length > 50)
                searchText = searchText[..50];

            (ErrorMessage, var result) = await _apiService.GetAsync<List<DisciplineShortInfo>>("Discipline",
                $"getOptionsInfo?holding={_holding}&eduLevel={_eduLevel}&course={_course}&semester={_semester}&code={searchText}",
                _accessToken);

            return result ?? new List<DisciplineShortInfo>();
        }
    }
}
