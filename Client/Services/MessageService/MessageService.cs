using System.Windows;

namespace Client.Services.MessageService
{
    public class MessageService : IMessageService
    {
        public bool ShowQuestion(string message, string caption = "Підтвердити дію")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
