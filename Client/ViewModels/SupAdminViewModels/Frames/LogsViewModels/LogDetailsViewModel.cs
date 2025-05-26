using Client.Models;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Client.ViewModels
{
    public partial class LogDetailsViewModel : ObservableRecipient, IPageViewModel
    {
        public string? PrettyOldJson { get; init; }
        public string? PrettyNewJson { get; init; }

        public IRelayCommand CloseCommand { get; }

        public LogDetailsViewModel(AuditLogInfo log, IRelayCommand closeCommand)
        {
            JsonSerializerOptions options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

            PrettyOldJson = GetValue(log.OldValue, options);
            PrettyNewJson = GetValue(log.NewValue, options);

            CloseCommand = closeCommand;
        }

        private static string? GetValue(string? value, JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            try
            {
                using var doc = JsonDocument.Parse(value);

                if (doc.RootElement.TryGetProperty("JsonValue", out var property))
                {
                    using var innerDoc = JsonDocument.Parse(property.GetString()!);
                    return JsonSerializer.Serialize(innerDoc, options);
                }
                else
                    return JsonSerializer.Serialize(doc, options);
            }
            catch
            {
                return value;
            }
        }
    }
}
