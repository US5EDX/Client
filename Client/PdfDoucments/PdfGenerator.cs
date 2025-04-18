using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Client.PdfDoucments
{
    public class PdfGenerator
    {
        public static async Task<string?> GeneratePdf(IDocument document, string savePath)
        {
            string? errorMessage = null;

            await Task.Run(() =>
            {
                try
                {
                    document.GeneratePdf(savePath);
                }
                catch
                {
                    errorMessage = "Не вдалось сформувати список за визначеним шляхом, спробуйте ще раз";
                }
            });

            return errorMessage;
        }
    }
}
