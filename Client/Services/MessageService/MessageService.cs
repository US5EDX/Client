using Microsoft.Win32;
using System.Windows;

namespace Client.Services.MessageService
{
    public class MessageService : IMessageService
    {
        public void ShowSuccessMessage(string message, string caption = "Успіх")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowQuestion(string message, string caption = "Підтвердити дію")
        {
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public string? ShowOpenFileDialog(string title, string filter, bool multiSelect = false)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = title;
            openFileDialog.Filter = filter;
            openFileDialog.Multiselect = multiSelect;

            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;

            return null;
        }

        public string? ShowSaveFileDialog(string title, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = title;
            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
                return saveFileDialog.FileName;

            return null;
        }
    }
}
