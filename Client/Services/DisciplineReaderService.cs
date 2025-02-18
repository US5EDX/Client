using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Pdf;
using Spire.Pdf.Utilities;
using System.Linq;
using System.Text;

namespace Client.Services
{
    public class DisciplineReaderService
    {
        private readonly HashSet<int> _selectedColumns;

        public DisciplineReaderService()
        {
            _selectedColumns = new HashSet<int>() { 0, 4, 5, 7, 9, 15, 16 };
        }

        public List<string> ReadDisciplinePdf(string pdfPath)
        {
            List<string> data = new List<string>();

            using (PdfDocument doc = new PdfDocument())
            {
                doc.LoadFromFile(pdfPath);

                using PdfTableExtractor extractor = new PdfTableExtractor(doc);
                PdfTable[] tableList = extractor.ExtractTable(0);

                if (tableList is null || tableList.Length == 0)
                    return data;

                PdfTable table = tableList[0];

                int rowsCount = table.GetRowCount();

                for (int i = 0; i < rowsCount; i++)
                {
                    if (!_selectedColumns.Contains(i))
                        continue;

                    data.Add(table.GetText(i, 1));
                }

                doc.Close();
            }

            GC.Collect();

            return data;
        }

        public List<string> ReadDisciplineDocx(string filePath)
        {
            List<string> data = new List<string>();

            using (Document doc = new Document())
            {
                doc.LoadFromFile(filePath);

                foreach (Section section in doc.Sections)
                {
                    var table = section.Tables[0];

                    if (table is null)
                        return data;

                    for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        if (!_selectedColumns.Contains(rowIndex))
                            continue;

                        var str = table.Rows[rowIndex].Cells[1].Paragraphs.Cast<Paragraph>()
                            .Aggregate(new StringBuilder(),
                            (strBuilder, value) => strBuilder.AppendLine(value.Text.Trim()), strBuilder =>
                            {
                                strBuilder.Length = strBuilder.Length - 2;
                                return strBuilder.ToString();
                            });

                        data.Add(str);
                    }
                }
            }

            return data;
        }
    }
}
