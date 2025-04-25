namespace Client.Services.MessageService
{
    public interface IMessageService
    {
        void ShowInfoMessage(string message, string caption = "Успіх");
        bool ShowQuestion(string message, string caption = "Підтвердити дію");
        string? ShowOpenFileDialog(string title, string filter, bool multiSelect = false);
        string? ShowSaveFileDialog(string title, string filter);
    }
}