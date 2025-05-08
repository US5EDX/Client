using Client.Models;
using Client.Services;
using Client.Stores;
using Client.ViewModels.Base;

namespace Client.ViewModels
{
    public partial class StudentChoicesViewModel(ApiService apiService, UserStore userStore) :
        FrameBaseViewModel(apiService, userStore)
    {
        public List<YearChoicesViewModel> GroupedChoices { get; init; } = [];

        public override async Task LoadContentAsync()
        {
            (ErrorMessage, var choices) =
                await _apiService.GetAsync<List<StudentChoiceInfo>>("Record",
                $"getMadeChoices", _userStore.AccessToken);

            if (HasErrorMessage)
                throw new Exception(ErrorMessage);

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
