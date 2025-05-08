using Client.Services;
using Client.Stores;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels.Base;

public abstract partial class ViewModelBase(ApiService apiService, UserStore userStore) : ObservableRecipient, IPageViewModel
{
    protected readonly ApiService _apiService = apiService;
    protected readonly UserStore _userStore = userStore;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLocked))]
    private bool _isWaiting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string? _errorMessage = default!;

    public bool IsNotLocked => !IsWaiting;

    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    protected async Task ExecuteWithWaiting(Func<Task> action)
    {
        ErrorMessage = string.Empty;
        IsWaiting = true;

        await action();

        IsWaiting = false;
    }
}
