using Client.Models;
using Client.Stores;
using ClosedXML.Excel;

namespace Client.ExcelDocuments
{
    public class StudentsRecordsExcelDocument
    {
        private readonly List<StudentWithAllRecordsInfo> _students;

        private readonly GroupInfoStore _groupInfoStore;

        public StudentsRecordsExcelDocument(List<StudentWithAllRecordsInfo> students, GroupInfoStore groupInfoStore)
        {
            _students = students;
            _groupInfoStore = groupInfoStore;
        }

        public async Task<string?> GenerateExcelAsync(string savePath)
        {
            return await Task.Run(() => GenerateExcel(savePath));
        }

        public string? GenerateExcel(string filePath)
        {
            int course = _groupInfoStore.Course;

            if (course == 0 && _groupInfoStore.AdmissionYear < DateTime.Today.Year)
                course = _groupInfoStore.DurationOfStudy;

            if (course == 0)
                return "Група ще не зарахована";

            if (course == _groupInfoStore.DurationOfStudy)
                course--;

            int firstSemester = _groupInfoStore.HasEnterChoise ? 0 : 2;
            int lastSemester = (course + (firstSemester == 0 ? 1 : 0)) * 2 + firstSemester;

            int endColumn = 0;
            int currColumn = 2;

            var semestersStartPosition = new Dictionary<int, (int StartPos, int Length)>();

            int headersRow = 2;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Student Records");

            worksheet.Cell(headersRow, 1).Value = "ПІБ студента";

            for (int sem = firstSemester + 1; sem <= lastSemester; sem++)
            {
                var disciplinesCount = sem % 2 == 0 ? _groupInfoStore.Parsemester : _groupInfoStore.Nonparsemester;

                semestersStartPosition[sem] = (currColumn, disciplinesCount);

                for (int j = 1; j <= disciplinesCount; j++)
                {
                    worksheet.Cell(headersRow, currColumn).Value = $"Дисципліна {j}";
                    currColumn++;
                }

                endColumn = currColumn - 1;

                worksheet.Range(1, semestersStartPosition[sem].StartPos, 1, endColumn).Merge().Value = $"Cеместр {sem}";
            }

            for (int i = 0; i < _students.Count; i++)
            {
                var student = _students[i];
                int row = i + headersRow + 1;

                worksheet.Cell(row, 1).Value = student.FullName;

                var semestersRecords = student.Records
                    .GroupBy(r => (r.EduYear - _groupInfoStore.AdmissionYear) * 2 + r.Semester);

                foreach (var semesterRecords in semestersRecords)
                {
                    int semester = semesterRecords.Key;

                    (currColumn, var count) = semestersStartPosition.GetValueOrDefault(semester, (-1, 0));

                    if (currColumn == -1)
                        continue;

                    foreach (var record in semesterRecords.Take(count))
                    {
                        worksheet.Cell(row, currColumn).Value = $"{record.DisciplineCode} {record.DisciplineName}";
                        currColumn++;
                    }
                }
            }

            StyleTable(worksheet, 1, 1, _students.Count + 2, endColumn);

            try
            {
                workbook.SaveAs(filePath);

                return null;
            }
            catch
            {
                return "Не вдалось зберегти файл";
            }
        }

        private void StyleTable(IXLWorksheet worksheet, int startRow, int startColumn, int endRow, int endColumn)
        {
            var table = worksheet.Range(startRow, startColumn, endRow, endColumn);

            table.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            table.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            table.Style.Alignment.WrapText = true;
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }
}
