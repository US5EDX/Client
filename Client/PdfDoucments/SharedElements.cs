using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Client.PdfDoucments
{
    public class SharedElements
    {
        public static void ComposeHeader(IContainer container, Action<IContainer> composeHeader)
        {
            container.Column(column =>
            {
                column.Item().ShowIf(currentPage => currentPage.PageNumber == 1).Element(composeHeader);
                column.Item().ShowIf(currentPage => currentPage.PageNumber > 1).Text($"Продовження документу")
                .AlignRight().FontSize(14).Bold().ParagraphSpacing(5);
            });
        }

        public static void ComposeFooter(IContainer container)
        {
            container.Text($"© Дніпровський національний університет імені Олеся Гончара {DateTime.Today.Year}")
                .FontSize(10)
                .AlignCenter();
        }

        public static void HeaderText(TextDescriptor text, string header)
        {
            text.Span(header).FontSize(18).Bold();
            text.AlignCenter();
        }

        public static void ItalicLabelText(TextDescriptor text, string label, string value)
        {
            text.Span($"{label}: ").Italic();
            text.Span(value);
        }

        public static void LabelTextRow(IContainer container, string label, string value)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(text => ItalicLabelText(text, label, value));
            });
        }

        public static void ComposeDateHeader(IContainer container)
        {
            LabelTextRow(container, "Дата формування документу", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
        }

        public static IContainer CellStyleHeader(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten2)
                .Border(1)
                .DefaultTextStyle(x => x.FontSize(11).Bold())
                .Padding(3)
                .AlignCenter()
                .AlignMiddle();
        }

        public static IContainer CellStyleBody(IContainer container, Color backgroundColor)
        {
            return container
                .Background(backgroundColor)
                .Border(1)
                .DefaultTextStyle(x => x.FontSize(11))
                .Padding(3)
                .AlignCenter()
                .AlignMiddle();
        }

        public static void DefineColumns(TableDescriptor table, List<(string Title, int Width)> columnsDef)
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var column in columnsDef)
                {
                    columns.RelativeColumn(column.Width);
                }
            });

            table.Header(header =>
            {
                foreach (var column in columnsDef)
                {
                    header.Cell().Element(CellStyleHeader).Text(column.Title);
                }
            });
        }

        public static void AddCell(TableDescriptor table, string text, Color backgroundColor)
        {
            table.Cell().Element(c => CellStyleBody(c, backgroundColor)).Text(text);
        }
    }
}
