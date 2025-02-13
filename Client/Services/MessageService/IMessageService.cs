namespace Client.Services.MessageService
{
    public interface IMessageService
    {
        bool ShowQuestion(string message, string caption = "Підтвердити дію");
    }
}