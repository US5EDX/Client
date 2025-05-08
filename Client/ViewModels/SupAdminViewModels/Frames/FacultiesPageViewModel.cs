using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class FacultiesPageViewModel : FrameBaseViewModel
    {
        public ObservableCollection<FacultyInfo> Faculties { get; init; }

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
        private string? _facultyName;

        public bool CanAddFaculty => SelectedFaculty is null && !string.IsNullOrEmpty(FacultyName);

        public bool IsFacultySelected => SelectedFaculty is not null && !string.IsNullOrEmpty(FacultyName);

        public Func<object, string, bool> Filter { get; init; }

        public FacultiesPageViewModel(ApiService apiService, UserStore userStore) : base(apiService, userStore)
        {
            Faculties = [];
            Filter = FilterFaculties;
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var faculties) =
                await _apiService.GetAsync<List<FacultyInfo>>("Faculty", "getFaculties", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var faculty in faculties ?? Enumerable.Empty<FacultyInfo>())
                Faculties.Add(faculty);
        }

        partial void OnSelectedFacultyChanged(FacultyInfo? value) => FacultyName = value?.FacultyName;

        [RelayCommand(CanExecute = nameof(CanAddFaculty))]
        private async Task AddFaculty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var newFaculty) =
                    await _apiService.PostAsync<FacultyInfo>("Faculty", "addFaculty", FacultyName, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Faculties.Add(newFaculty);
                    FacultyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsFacultySelected))]
        private async Task UpdateFaculty()
        {
            if (SelectedFaculty.FacultyName == FacultyName) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<FacultyInfo>("Faculty", "updateFaculty",
                    new FacultyInfo { FacultyId = SelectedFaculty.FacultyId, FacultyName = FacultyName }, _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedFaculty.FacultyName = FacultyName;
                    SelectedFaculty = null;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsFacultySelected))]
        private async Task DeleteFaculty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>("Faculty", $"deleteFaculty/{SelectedFaculty.FacultyId}",
                    _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Faculties.Remove(SelectedFaculty);
                    SelectedFaculty = null;
                }
            });
        }

        private bool FilterFaculties(object faculty, string filter)
        {
            if (faculty is not FacultyInfo facultyInfo) return false;

            return facultyInfo.FacultyName.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
