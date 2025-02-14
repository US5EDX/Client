using Client.Models;
using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class SpecialtiesPageViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;
        private readonly ObservableCollection<SpecialtyInfo> _specialties;

        public IEnumerable<SpecialtyInfo> Specialties => _specialties;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSpecialtySelected))]
        [NotifyPropertyChangedFor(nameof(CanAddSpecialty))]
        [NotifyCanExecuteChangedFor(nameof(UpdateSpecialtyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DeleteSpecialtyCommand))]
        private SpecialtyInfo? _selectedSpecialty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanAddSpecialty))]
        [NotifyPropertyChangedFor(nameof(IsSpecialtySelected))]
        [NotifyCanExecuteChangedFor(nameof(AddSpecialtyCommand))]
        [NotifyCanExecuteChangedFor(nameof(UpdateSpecialtyCommand))]
        private string _specialtyName;

        public string FacultyName { get; init; }

        [ObservableProperty]
        private bool _isWaiting;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
        private string? _errorMessage = default!;

        public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

        public bool CanAddSpecialty => SelectedSpecialty is null && !string.IsNullOrEmpty(SpecialtyName);
        public bool IsSpecialtySelected => SelectedSpecialty is not null && !string.IsNullOrEmpty(SpecialtyName);

        public Func<object, string, bool> Filter { get; init; }

        public SpecialtiesPageViewModel(ApiService apiService, UserStore userStore)
        {
            _apiService = apiService;
            _userStore = userStore;

            _specialties = new ObservableCollection<SpecialtyInfo>();

            if (_userStore.WorkerInfo is null || _userStore.Role != 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            FacultyName = _userStore.WorkerInfo.Faculty.FacultyName;

            Filter = FilterFaculties;
        }

        partial void OnSelectedSpecialtyChanged(SpecialtyInfo value)
        {
            if (IsWaiting)
                return;

            SpecialtyName = value?.SpecialtyName;
        }

        public async Task LoadContentAsync()
        {
            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<ObservableCollection<SpecialtyInfo>>("Specialty",
                $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            _specialties.Clear();

            foreach (var specialty in specialties ?? Enumerable.Empty<SpecialtyInfo>())
                _specialties.Add(specialty);
        }

        [RelayCommand(CanExecute = nameof(CanAddSpecialty))]
        private async Task AddSpecialty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var newSpecialty) =
                    await _apiService.PostAsync<SpecialtyInfo>("Specialty", "addSpecialty", new SpecialtyInfo()
                    {
                        SpecialtyName = SpecialtyName,
                        FacultyId = _userStore.WorkerInfo.Faculty.FacultyId
                    },
                    _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _specialties.Add(newSpecialty);
                    SpecialtyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsSpecialtySelected))]
        private async Task UpdateSpecialty()
        {
            if (SelectedSpecialty.SpecialtyName == SpecialtyName)
                return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<SpecialtyInfo>("Specialty", "updateSpecialty",
                    new SpecialtyInfo
                    {
                        SpecialtyId = SelectedSpecialty.SpecialtyId,
                        SpecialtyName = SpecialtyName,
                    },
                    _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedSpecialty.SpecialtyName = SpecialtyName;
                    SelectedSpecialty = null;
                    SpecialtyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsSpecialtySelected))]
        private async Task DeleteSpecialty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.DeleteAsync<object>("Specialty", $"deleteSpecialty/{SelectedSpecialty.SpecialtyId}",
                    _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    _specialties.Remove(SelectedSpecialty);
                    SelectedSpecialty = null;
                    SpecialtyName = string.Empty;
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
            if (faculty is not SpecialtyInfo specialtyInfo)
                return false;
            return specialtyInfo.SpecialtyName.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
