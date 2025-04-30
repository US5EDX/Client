using Client.Models;
using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels
{
    public partial class StudentChoicesViewModel : ObservableRecipient, IFrameViewModel
    {
        private readonly ApiService _apiService;
        private readonly UserStore _userStore;

        public List<YearChoicesViewModel> GroupedChoices { get; init; }

        public StudentChoicesViewModel(ApiService apiService, UserStore userStore)
        {
            _apiService = apiService;
            _userStore = userStore;

            GroupedChoices = new List<YearChoicesViewModel>();
        }

        public async Task LoadContentAsync()
        {
            (var errorMessage, var choices) =
                await _apiService.GetAsync<List<StudentChoiceInfo>>("Record",
                $"getMadeChoices", _userStore.AccessToken);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage);

            var grouped = choices?
            .GroupBy(c => c.Holding)
            .Select(g => new YearChoicesViewModel
            {
                Holding = g.Key,
                Semesters =
                [
                    new SemesterChoicesViewModel
                    {
                        Semester = 1,
                        Choices = g.Where(x => x.Semester == 1).Select(x => x.GetDisplayInfo()).ToList()
                    },
                    new SemesterChoicesViewModel
                    {
                        Semester = 2,
                        Choices = g.Where(x => x.Semester == 2).Select(x => x.GetDisplayInfo()).ToList()
                    }
                ]
            }) ?? [];

            GroupedChoices.AddRange(grouped);
        }
    }
}
