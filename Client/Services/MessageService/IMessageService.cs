namespace Client.Services.MessageService
{
    public interface IMessageService
    {
        bool ShowQuestion(string message, string caption = "Підтвердити дію");
        string? ShowOpenFileDialog(string title, string filter, bool multiSelect = false);
        string? ShowSaveFileDialog(string title, string filter);
    }
}