using Client.Services;
using Client.Stores;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Client.ViewModels.Base
{
    public abstract partial class PaginationFrameViewModelBase(ApiService apiService, UserStore userStore) :
        FrameBaseViewModelWithModal(apiService, userStore)
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNextPageEnabled))]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        [NotifyPropertyChangedFor(nameof(IsPreviousPageEnabled))]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int _currentPage;

        [ObservableProperty]
        private int _totalPages;

        public int PageSize { get; init; } = 50;

        public bool IsNextPageEnabled => CurrentPage < TotalPages;
        public bool IsPreviousPageEnabled => CurrentPage > 1;

        public Func<object, string, bool> Filter { get; init; } = null!;

        protected abstract Task LoadDataAsync(int page);

        protected async Task LoadTotalPagesAsync(string nav, string endpoint)
        {
            (ErrorMessage, var totalSize) =
                await _apiService.GetAsync<int>(nav, endpoint, _userStore.AccessToken);

            if (HasErrorMessage)
                return;

            TotalPages = (int)Math.Ceiling((double)totalSize / PageSize);
            CurrentPage = 0;
        }

        [RelayCommand(CanExecute = nameof(IsNextPageEnabled))]
        protected virtual async Task NextPage() => await LoadDataAsync(CurrentPage + 1);

        [RelayCommand(CanExecute = nameof(IsPreviousPageEnabled))]
        protected virtual async Task PreviousPage() => await LoadDataAsync(CurrentPage - 1);
    }
}
