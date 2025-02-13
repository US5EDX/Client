using Client.Models;
using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class FacultiesPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly ObservableCollection<FacultyInfo> _faculties;

        public IEnumerable<FacultyInfo> Faculties => _faculties;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFacultySelected))]
        [NotifyPropertyChangedFor(nameof(CanAddFaculty))]
        [NotifyCanExecuteChangedFor(nameof(UpdateFacultyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteFacultyCommand))]
        private FacultyInfo? _selectedFaculty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanAddFaculty))]
        [NotifyPropertyChangedFor(nameof(IsFacultySelected))]
        [NotifyCanExecuteChangedFor(nameof(AddFacultyCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateFacultyCommand))]
        private string _facultyName;

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanAddFaculty => SelectedFaculty is null && !string.IsNullOrEmpty(FacultyName);
        public bool IsFacultySelected => SelectedFaculty is not null && !string.IsNullOrEmpty(FacultyName);

        public Func<object, string, bool> Filter { get; init; }

        public FacultiesPageViewModel(ApiService apiService, UserStore userStore)
        {
            _apiService = apiService;
            _userStore = userStore;

            _faculties = new ObservableCollection<FacultyInfo>();

            Filter = FilterFaculties;
        }

        partial void OnSelectedFacultyChanged(FacultyInfo value)
        {
            if (IsWaiting)
                return;

            FacultyName = value?.FacultyName;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var faculties) =
                await _apiService.GetAsync<ObservableCollection<FacultyInfo>>("Faculty", "getFaculties", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _faculties.Clear();

            foreach (var faculty in faculties ?? Enumerable.Empty<FacultyInfo>())
                _faculties.Add(faculty);
        }

        [RelayCommand(CanExecute = nameof(CanAddFaculty))]
        private async Task AddFaculty()
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var newFaculty) =
                    await _apiService.PostAsync<FacultyInfo>("Faculty", "addFaculty", FacultyName, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _faculties.Add(newFaculty);
                    FacultyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsFacultySelected))]
        private async Task UpdateFaculty()
        {
            if (SelectedFaculty.FacultyName == FacultyName)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<FacultyInfo>("Faculty", "updateFaculty",
                    new FacultyInfo { FacultyId = SelectedFaculty.FacultyId, FacultyName = FacultyName }, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedFaculty.FacultyName = FacultyName;
                    SelectedFaculty = null;
                    FacultyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsFacultySelected))]
        private async Task DeleteFaculty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>("Faculty", $"deleteFaculty/{SelectedFaculty.FacultyId}", _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _faculties.Remove(SelectedFaculty);
                    SelectedFaculty = null;
                    FacultyName = string.Empty;
                }
            });
        }

        private async Task ExecuteWithWaiting(Func<Task> action)
        {
            ErrorMessage = string.Empty;
            IsWaiting = true;

            await action();

            IsWaiting = false;
        }

        private bool FilterFaculties(object faculty, string filter)
        {
            if (faculty is not FacultyInfo facultyInfo)
                return false;
            return facultyInfo.FacultyName.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
