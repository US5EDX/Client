using Client.ViewModels.Interfaces;

public interface IFrameViewModel : IPageViewModel
{
    Task LoadContentAsync();
}
