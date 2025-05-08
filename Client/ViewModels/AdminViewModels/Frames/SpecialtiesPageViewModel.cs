using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Client.ViewModels
{
    public partial class SpecialtiesPageViewModel : FrameBaseViewModel
    {
        public ObservableCollection<SpecialtyInfo> Specialties { get; init; }

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
        private string? _specialtyName;

        public string FacultyName { get; init; }

        public bool CanAddSpecialty => SelectedSpecialty is null && !string.IsNullOrEmpty(SpecialtyName);
        public bool IsSpecialtySelected => SelectedSpecialty is not null && !string.IsNullOrEmpty(SpecialtyName);

        public Func<object, string, bool> Filter { get; init; }

        public SpecialtiesPageViewModel(ApiService apiService, UserStore userStore) : base(apiService, userStore)
        {
            if (_userStore.WorkerInfo is null || _userStore.Role != 2)
                throw new UnauthorizedAccessException("У доступі відмовлено");

            Specialties = [];

            FacultyName = _userStore.WorkerInfo.Faculty.FacultyName;

            Filter = FilterSpecialties;
        }

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var specialties) =
                await _apiService.GetAsync<List<SpecialtyInfo>>("Specialty",
                $"getSpecialties/{_userStore.WorkerInfo.Faculty.FacultyId}", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

            foreach (var specialty in specialties ?? Enumerable.Empty<SpecialtyInfo>())
                Specialties.Add(specialty);
        }

        partial void OnSelectedSpecialtyChanged(SpecialtyInfo? value) => SpecialtyName = value?.SpecialtyName;

        [RelayCommand(CanExecute = nameof(CanAddSpecialty))]
        private async Task AddSpecialty()
        {
            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, var newSpecialty) =
                    await _apiService.PostAsync<SpecialtyInfo>("Specialty", "addSpecialty",
                    InithializeInstance(), _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    Specialties.Add(newSpecialty);
                    SpecialtyName = string.Empty;
                }
            });
        }

        [RelayCommand(CanExecute = nameof(IsSpecialtySelected))]
        private async Task UpdateSpecialty()
        {
            if (SelectedSpecialty.SpecialtyName == SpecialtyName) return;

            await ExecuteWithWaiting(async () =>
            {
                (ErrorMessage, _) =
                    await _apiService.PutAsync<SpecialtyInfo>("Specialty", "updateSpecialty",
                    InithializeInstance(), _userStore.AccessToken);

                if (!HasErrorMessage)
                {
                    SelectedSpecialty.SpecialtyName = SpecialtyName;
                    SelectedSpecialty = null;
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
                    Specialties.Remove(SelectedSpecialty);
                    SelectedSpecialty = null;
                }
            });
        }

        private bool FilterSpecialties(object faculty, string filter)
        {
            if (faculty is not SpecialtyInfo specialtyInfo) return false;

            return specialtyInfo.SpecialtyName.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private SpecialtyInfo InithializeInstance()
        {
            return new SpecialtyInfo
            {
                SpecialtyId = SelectedSpecialty.SpecialtyId,
                SpecialtyName = SpecialtyName,
            };
        }
    }
}
