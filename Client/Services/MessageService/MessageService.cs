using Microsoft.Win32;
using System.Windows;

namespace Client.Services.MessageService
{
    public class MessageService : IMessageService
    {
        public bool ShowQuestion(string message, string caption = "Підтвердити дію")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public string? ShowFileDialog(string title, string filter, bool multiSelect = false)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = title;
            openFileDialog.Filter = filter;
            openFileDialog.Multiselect = multiSelect;

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;

            return null;
        }
    }
}
