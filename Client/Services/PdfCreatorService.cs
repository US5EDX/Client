﻿using Client.Models;
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
        public bool SaveSignedStudents(string path, IEnumerable<RecordWithStudentInfo> studentInfos, string disciplineName, string semester, int total)
        {
            using (PdfWriter writer = new PdfWriter(path))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);

                    PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");
                    string[] headers = { "Email", "ПІБ", "Факультет", "Група", "Семестр" };

                    document.Add(new Paragraph("Список")
                            .SetFont(font)
                            .SimulateBold()
                            .SetFontSize(18)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(10));

                    document.Add(CreateInfoParagraph("Дисципліна", disciplineName, font));
                    document.Add(CreateInfoParagraph("Семестр", semester, font));
                    document.Add(CreateInfoParagraph("Кількість студентів", total.ToString(), font));
                    document.Add(CreateInfoParagraph("Дата формування документа", DateTime.Now.ToString("dd.MM.yyyy"), font));

                    Table table = new Table(headers.Length);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    foreach (var header in headers)
                    {
                        table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFont(font))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(TextAlignment.CENTER));
                    }

                    foreach (var record in studentInfos)
                    {
                        Color rowColor = record.Approved ? ColorConstants.GREEN : ColorConstants.RED;

                        table.AddCell(CreateCell(record.Email, rowColor, font));
                        table.AddCell(CreateCell(record.FullName, rowColor, font));
                        table.AddCell(CreateCell(record.FacultyName, rowColor, font));
                        table.AddCell(CreateCell(record.GroupCode, rowColor, font));
                        table.AddCell(CreateCell(record.Semester == 1 ? "Непарний" : "Парний", rowColor, font));
                    }

                    document.Add(table);
                    document.Close();
                }
            }

            return true;
        }

        public bool SaveStudentsRecords(string path, IEnumerable<StudentRecordsInfo> studentInfos,
            string groupCode, int nonparsemester, int parsemester)
        {
            using (PdfWriter writer = new PdfWriter(path))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    pdf.SetDefaultPageSize(PageSize.A3.Rotate());

                    Document document = new Document(pdf);

                    PdfFont font = PdfFontFactory.CreateFont("Times New Roman.ttf");

                    List<string> headers = ["ПІБ"];

                    for (int i = 0; i < nonparsemester; i++)
                        headers.Add($"Осінній семестр");

                    for (int i = 0; i < parsemester; i++)
                        headers.Add($"Весняний семестр");

                    document.Add(new Paragraph("Список")
                            .SetFont(font)
                            .SimulateBold()
                            .SetFontSize(18)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetMarginBottom(10));

                    document.Add(CreateInfoParagraph("Група", groupCode, font));
                    document.Add(CreateInfoParagraph("Дата формування документа", DateTime.Now.ToString("dd.MM.yyyy"), font));

                    Table table = new Table(headers.Count);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    foreach (var header in headers)
                    {
                        table.AddHeaderCell(new Cell().Add(new Paragraph(header).SetFont(font))
                            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                            .SetTextAlignment(TextAlignment.CENTER));
                    }

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

                    document.Add(table);
                    document.Close();
                }
            }

            return true;
        }

        private static Paragraph CreateInfoParagraph(string identifierText, string valueText, PdfFont font)
        {
            return new Paragraph()
                .Add(new Text(identifierText + ": ").SetFont(font).SimulateItalic())
                .Add(new Text(valueText).SetFont(font))
                .SetFontSize(12)
                .SetMarginBottom(5);
        }

        private static Cell CreateCell(string text, Color bgColor, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetBackgroundColor(bgColor)
                .SetTextAlignment(TextAlignment.CENTER);
        }
    }
}
