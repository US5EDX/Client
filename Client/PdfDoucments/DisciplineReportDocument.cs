using Client.Converters;
using Client.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Client.PdfDoucments
{
    public class DisciplineReportDocument : IDocument
    {
        private readonly List<DisciplinePrintInfo> _disciplines;
        private readonly Dictionary<byte, List<DisciplinePrintInfo>> _groupedDisciplines;
        private readonly string _facultyName;
        private readonly string _catalogType;
        private readonly short _eduYear;
        private readonly int _semester;
        private readonly DisciplineStatusThresholds _thresholds;

        private readonly EduLevelToEduLevelNameConverter _eduLevelConverter;
        private readonly SemesterToSemesterNameConverter _semesterConverter;

        public DisciplineReportDocument(List<DisciplinePrintInfo> disciplines, string facultyName,
            string catalogType, short eduYear, int semester, DisciplineStatusThresholds thresholds)
            : this(facultyName, catalogType, eduYear, semester, thresholds)
        {
            _disciplines = disciplines;
        }

        public DisciplineReportDocument(Dictionary<byte, List<DisciplinePrintInfo>> groupedDisciplines, string facultyName,
            string catalogType, short eduYear, int semester, DisciplineStatusThresholds thresholds)
            : this(facultyName, catalogType, eduYear, semester, thresholds)
        {
            _groupedDisciplines = groupedDisciplines;
        }

        private DisciplineReportDocument(string facultyName, string catalogType, short eduYear, int semester,
            DisciplineStatusThresholds thresholds)
        {
            _facultyName = facultyName;
            _catalogType = catalogType;
            _eduYear = eduYear;
            _semester = semester;
            _thresholds = thresholds;

            _eduLevelConverter = new EduLevelToEduLevelNameConverter();
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
                page.Content().Element(_disciplines is null ? ComposeContentWithGrouping : ComposeContent);
                page.Footer().Element(SharedElements.ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(5);

                column.Item().Text(text => SharedElements.HeaderText(text, "Відомість"));

                column.Item().Element(container => SharedElements.LabelTextRow(container, "Факультет", _facultyName));
                column.Item().Element(container => SharedElements.LabelTextRow(container, "Тип каталогу", _catalogType));
                column.Item().Element(container => SharedElements.LabelTextRow(container, "Навчальний рік", $"{_eduYear}/{_eduYear + 1}"));
                column.Item().Element(container => SharedElements.LabelTextRow(container, "Семестр", _semester == 1 ? "Осінній" : "Весняний"));
                column.Item().Element(SharedElements.ComposeDateHeader);

                column.Item().PaddingTop(10).Text(text => SharedElements.ItalicLabelText(text, "Пороги",
                    $"<{_thresholds.NotEnough} — Недостатньо, <{_thresholds.PartiallyFilled} — Умовно набрана, інше — Набрана"));
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(15).Table(table => CreateTable(table, _disciplines));
        }

        private void ComposeContentWithGrouping(IContainer container)
        {
            container.Column(column =>
            {
                foreach (var group in _groupedDisciplines)
                {
                    column.Item().PaddingTop(10)
                    .Text((string)_eduLevelConverter.Convert(group.Key, null, null, null))
                    .AlignCenter().FontSize(12).Bold();

                    column.Item().PaddingTop(5).Table(table => CreateTable(table, group.Value));
                }
            });
        }

        private void CreateTable(TableDescriptor table, List<DisciplinePrintInfo> disciplines)
        {
            var columns = new List<(string Title, int Width)>(10)
            {
                ("Код", 2),
                ("Назва", 3),
                ("Кількість студентів", 1),
                ("Спеціальність", 1),
                ("Курс", 2),
                ("Семестр", 1),
                ("Мінімум", 1),
                ("Максимум", 1),
                ("Відкрита до набору", 1)
            };

            if (_disciplines is not null)
                columns.Insert(4, ("Рівень ВО", 1));

            SharedElements.DefineColumns(table, columns);

            foreach (var item in disciplines)
            {
                string color = Color.FromHex(item.ColorStatus);

                SharedElements.AddCell(table, item.DisciplineCode, color);
                SharedElements.AddCell(table, item.DisciplineName, color);
                SharedElements.AddCell(table, item.StudentsCount.ToString(), color);
                SharedElements.AddCell(table, item.SpecialtyName ?? "-", color);

                if (_disciplines is not null)
                    SharedElements.AddCell(table, (string)_eduLevelConverter.Convert(item.EduLevel, null, null, null), color);

                SharedElements.AddCell(table, item.Course, color);
                SharedElements.AddCell(table, (string)_semesterConverter.Convert(item.Semester, null, null, null), color);
                SharedElements.AddCell(table, item.MinCount == 0 ? "-" : item.MinCount.ToString(), color);
                SharedElements.AddCell(table, item.MaxCount == 0 ? "-" : item.MaxCount.ToString(), color);
                SharedElements.AddCell(table, item.IsOpen ? "Так" : "Ні", color);
            }
        }
    }
}
