using Client.Converters;
using Client.Models;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Client.Services
{
    public class PdfCreatorService
    {
        public bool SaveSignedStudents(string path, IEnumerable<RecordWithStudentInfo> studentInfos, string disciplineName,
            string semester, int total)
        {
            PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");

            string[] headers = ["Email", "ПІБ", "Факультет", "Група", "Семестр"];

            List<Paragraph> paragraphs = new List<Paragraph>
            {
                CreateInfoParagraph("Дисципліна", disciplineName, font),
                CreateInfoParagraph("Семестр", semester, font),
                CreateInfoParagraph("Кількість студентів", total.ToString(), font)
            };

            Table table = CreateTable(headers, font);

            foreach (var record in studentInfos)
            {
                Color rowColor = record.Approved ? ColorConstants.GREEN : ColorConstants.RED;

                table.AddCell(CreateCell(record.Email, rowColor, font));
                table.AddCell(CreateCell(record.FullName, rowColor, font));
                table.AddCell(CreateCell(record.FacultyName, rowColor, font));
                table.AddCell(CreateCell(record.GroupCode, rowColor, font));
                table.AddCell(CreateCell(record.Semester == 1 ? "Непарний" : "Парний", rowColor, font));
            }

            CreatePdf(path, font, "Список", paragraphs, (document) =>
            {
                document.Add(table);
            });

            return true;
        }

        public bool SaveStudentsRecords(string path, IEnumerable<StudentRecordsInfo> studentInfos,
            string groupCode, int nonparsemester, int parsemester)
        {
            PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");

            List<string> headers = ["ПІБ"];

            for (int i = 0; i < nonparsemester; i++)
                headers.Add($"Осінній семестр");

            for (int i = 0; i < parsemester; i++)
                headers.Add($"Весняний семестр");

            List<Paragraph> paragraphs = new List<Paragraph>
            {
                CreateInfoParagraph("Група", groupCode, font)
            };

            Table table = CreateTable(headers.ToArray(), font);

            foreach (var record in studentInfos)
            {
                table.AddCell(CreateCell(record.FullName, ColorConstants.WHITE, font));

                for (int i = 0; i < nonparsemester; i++)
                {
                    var recordInfo = record.Nonparsemester.ElementAtOrDefault(i);
                    table.AddCell(CreateCell(recordInfo?.CodeName ?? "Не обрано",
                        recordInfo is null ? ColorConstants.WHITE :
                        (recordInfo.Approved ? ColorConstants.GREEN : ColorConstants.RED), font));
                }

                for (int i = 0; i < parsemester; i++)
                {
                    var recordInfo = record.Parsemester.ElementAtOrDefault(i);
                    table.AddCell(CreateCell(recordInfo?.CodeName ?? "Не обрано",
                        recordInfo is null ? ColorConstants.WHITE :
                        (recordInfo.Approved ? ColorConstants.GREEN : ColorConstants.RED), font));
                }
            }

            CreatePdf(path, font, "Список", paragraphs, (document) =>
            {
                document.Add(table);
            });

            return true;
        }

        public bool SaveDisciplinesInfo(string path, List<DisciplinePrintInfo> disciplinesInfo,
            string facultyName, string catalogType, short eduYear, int choosenSemester, DisciplineStatusThresholds thresholds)
        {
            PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");

            string[] headers = ["Код дисципліни", "Назва", "Кількість студентів", "Спеціальність",
                        "Рівень ВО", "Курс", "Семестр", "Мінімум", "Максимум", "Відкрита до набору"];

            var paragraphs = CreateDisciplineParagraphs(font, eduYear, catalogType, (byte)choosenSemester, facultyName, thresholds);

            Table table = CreateDisciplinesTable(headers, font, disciplinesInfo);

            CreatePdf(path, font, "Відомість", paragraphs, (document) =>
            {
                document.Add(table);
            });

            return true;
        }

        public bool SaveDisciplinesInfo(string path, Dictionary<byte, List<DisciplinePrintInfo>> disciplinesInfo,
            string facultyName, string catalogType, short eduYear, int choosenSemester, DisciplineStatusThresholds thresholds)
        {
            PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");

            string[] headers = ["Код дисципліни", "Назва", "Кількість студентів", "Спеціальність",
                        "Курс", "Семестр", "Мінімум", "Максимум", "Відкрита до набору"];

            var paragraphs = CreateDisciplineParagraphs(font, eduYear, catalogType, (byte)choosenSemester, facultyName, thresholds);

            var eduLevelConverter = new EduLevelToEduLevelNameConverter();

            CreatePdf(path, font, "Відомість", paragraphs, (document) =>
            {
                foreach (var pair in disciplinesInfo)
                {
                    document.Add(new Paragraph((string)eduLevelConverter.Convert(pair.Key, null, null, null))
                        .SetFont(font)
                        .SimulateBold()
                        .SetFontSize(14)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(5));

                    Table table = CreateDisciplinesTable(headers, font, pair.Value, false);

                    document.Add(table);
                }
            });

            return true;
        }

        private void CreatePdf(string path, PdfFont font, string mainHeader,
            List<Paragraph> paragraphs, Action<Document> tablesAddAction)
        {
            using (PdfWriter writer = new PdfWriter(path))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    pdf.SetDefaultPageSize(PageSize.A4.Rotate());

                    Document document = new Document(pdf);

                    document.Add(new Paragraph(mainHeader)
                            .SetFont(font)
                            .SimulateBold()
                            .SetFontSize(18)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(10));

                    foreach (var paragraph in paragraphs)
                        document.Add(paragraph);

                    document.Add(CreateInfoParagraph("Дата та час формування документа", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), font));

                    tablesAddAction(document);

                    document.Close();
                }
            }
        }

        private Paragraph CreateInfoParagraph(string identifierText, string valueText, PdfFont font)
        {
            return new Paragraph()
                .Add(new Text(identifierText + ": ").SetFont(font).SimulateItalic())
                .Add(new Text(valueText).SetFont(font))
                .SetFontSize(12)
                .SetMarginBottom(5);
        }

        private Table CreateTable(string[] headers, PdfFont font)
        {
            Table table = new Table(headers.Length);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            foreach (var header in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFont(font))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
            }

            return table;
        }

        private Cell CreateCell(string text, Color bgColor, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetBackgroundColor(bgColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);
        }

        private List<Paragraph> CreateDisciplineParagraphs(PdfFont font, short eduYear, string catalogType,
            byte choosenSemester, string facultyName, DisciplineStatusThresholds thresholds)
        {
            return new List<Paragraph>()
            {
                CreateInfoParagraph("Навчальний рік", $"{eduYear}/{eduYear + 1}", font),
                CreateInfoParagraph("Тип каталогу", catalogType, font),
                CreateInfoParagraph("Відомість на семестр", choosenSemester == 1 ? "Осінній" : "Весняний", font),
                CreateInfoParagraph("Факультет", facultyName, font),
                CreateInfoParagraph("Порогові значення кількості студентів для дисципліни станом на дату формування документа",
                $"Недостатньо, якщо < {thresholds.NotEnough}; Умовно набрана, якщо < {thresholds.PartiallyFilled}; " +
                $"Набрана при при більшій кількості", font)
            };
        }

        private Table CreateDisciplinesTable(string[] headers, PdfFont font, List<DisciplinePrintInfo> disciplinesInfo,
            bool showEduLevel = true)
        {
            var table = CreateTable(headers, font);

            var catalogConverter = new CatalogTypeToCatalogNameConverter();
            var eduLevelConverter = showEduLevel ? new EduLevelToEduLevelNameConverter() : null;
            var semesterConverter = new SemesterToSemesterNameConverter();

            foreach (var record in disciplinesInfo)
            {
                var rowColor = WebColors.GetRGBColor(record.ColorStatus);

                table.AddCell(CreateCell(record.DisciplineCode, rowColor, font));
                table.AddCell(CreateCell(record.DisciplineName, rowColor, font));
                table.AddCell(CreateCell(record.StudentsCount.ToString(), rowColor, font));
                table.AddCell(CreateCell(record.SpecialtyName ?? "-", rowColor, font));
                if (showEduLevel)
                    table.AddCell(CreateCell(
                        (string)eduLevelConverter.Convert(record.EduLevel, null, null, null),
                        rowColor, font));
                table.AddCell(CreateCell(record.Course.ToString(), rowColor, font));
                table.AddCell(CreateCell(
                    (string)semesterConverter.Convert(record.Semester, null, null, null),
                    rowColor, font));
                table.AddCell(CreateCell(record.MinCount == 0 ? "-" : record.MinCount.ToString(), rowColor, font));
                table.AddCell(CreateCell(record.MaxCount == 0 ? "-" : record.MaxCount.ToString(), rowColor, font));
                table.AddCell(CreateCell(record.IsOpen ? "Так" : "Ні", rowColor, font));
            }

            return table;
        }
    }
}
