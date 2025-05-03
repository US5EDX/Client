using Client.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Client.PdfDoucments
{
    public class StudentsRecordsDocument : IDocument
    {
        private readonly IEnumerable<StudentRecordsInfo> _studentInfos;
        private readonly string _groupCode;
        private readonly int _nonparsemester;
        private readonly int _parsemester;

        public StudentsRecordsDocument(IEnumerable<StudentRecordsInfo> studentInfos,
            string groupCode, int nonparsemester, int parsemester)
        {
            _studentInfos = studentInfos;
            _groupCode = groupCode;
            _nonparsemester = nonparsemester;
            _parsemester = parsemester;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A3.Landscape());
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

                column.Item().Element(container => SharedElements.LabelTextRow(container, "Група", _groupCode));
                column.Item().Element(SharedElements.ComposeDateHeader);
            });
        }

        private void ComposeContent(IContainer container)
        {
            var columns = new List<(string Title, int Width)>(_nonparsemester + _parsemester + 1)
            {
                ("ПІБ", 1)
            };

            for (int i = 0; i < _nonparsemester; i++)
                columns.Add(("Осінній семестр", 2));

            for (int i = 0; i < _parsemester; i++)
                columns.Add(("Весняний семестр", 2));

            container.PaddingTop(15).Table(table =>
            {
                SharedElements.DefineColumns(table, columns);

                foreach (var item in _studentInfos)
                {
                    SharedElements.AddCell(table, item.FullName, Colors.White);

                    for (int i = 0; i < _nonparsemester; i++)
                    {
                        var recordInfo = item.Nonparsemester.ElementAtOrDefault(i);

                        SharedElements.AddCell(table, recordInfo?.CodeName ?? "Не обрано",
                            recordInfo is null ? Colors.White : (recordInfo.Approved == 1 ? Colors.Green.Lighten4 :
                            recordInfo.Approved == 2 ? Colors.Yellow.Lighten4 : Colors.Red.Lighten4));
                    }

                    for (int i = 0; i < _parsemester; i++)
                    {
                        var recordInfo = item.Parsemester.ElementAtOrDefault(i);

                        SharedElements.AddCell(table, recordInfo?.CodeName ?? "Не обрано",
                           recordInfo is null ? Colors.White : (recordInfo.Approved == 1 ? Colors.Green.Lighten4 :
                           recordInfo.Approved == 2 ? Colors.Yellow.Lighten4 : Colors.Red.Lighten4));
                    }
                }
            });
        }
    }
}
