using Client.Converters;
using Client.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Client.PdfDoucments
{
    public class SignedStudentsReportDocument : IDocument
    {
        private readonly IEnumerable<RecordWithStudentInfo> _studentInfos;
        private readonly string _disciplineName;
        private readonly string _semester;
        private readonly int _total;

        private readonly SemesterToSemesterNameConverter _semesterConverter;

        public SignedStudentsReportDocument(IEnumerable<RecordWithStudentInfo> studentInfos, string disciplineName,
            string semester, int total)
        {
            _studentInfos = studentInfos;
            _disciplineName = disciplineName;
            _semester = semester;
            _total = total;

            _semesterConverter = new SemesterToSemesterNameConverter();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4.Landscape());
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman"));

                page.Header().Element(container => SharedElements.ComposeHeader(container, ComposeHeader));
                page.Content().Element(ComposeContent);
                page.Footer().Element(SharedElements.ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(5);

                column.Item().Text(text => SharedElements.HeaderText(text, "Список"));

                column.Item().Element(container => SharedElements.LabelTextRow(container, "Дисципліна", _disciplineName));
                column.Item().Element(container => SharedElements.LabelTextRow(container, "Семестр", _semester));
                column.Item().Element(container => SharedElements.LabelTextRow(container, "Кількість студентів", _total.ToString()));
                column.Item().Element(SharedElements.ComposeDateHeader);
            });
        }

        private void ComposeContent(IContainer container)
        {
            var columns = new List<(string Title, int Width)>(5)
            {
                ("Email", 2),
                ("ПІБ", 2),
                ("Факультет", 1),
                ("Група", 1),
                ("Семестр", 1),
            };

            container.PaddingTop(15).Table(table =>
            {
                SharedElements.DefineColumns(table, columns);

                foreach (var item in _studentInfos)
                {
                    string color = item.Approved ? Colors.Green.Lighten4 : Colors.Red.Lighten4;

                    SharedElements.AddCell(table, item.Email, color);
                    SharedElements.AddCell(table, item.FullName, color);
                    SharedElements.AddCell(table, item.FacultyName, color);
                    SharedElements.AddCell(table, item.GroupCode, color);
                    SharedElements.AddCell(table, (string)_semesterConverter.Convert(item.Semester, null, null, null), color);
                }
            });
        }
    }
}
