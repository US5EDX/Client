using Client.Models;
using ClosedXML.Excel;
using System.IO;

namespace Client.Services
{
    public class StudentsReaderService
    {
        public List<StudentExcelInfo> GetStudentsInfo(string filePath)
        {
            var students = new List<StudentExcelInfo>();

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не знайдено.");

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    throw new Exception("Файл не містить листів.");

                int rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                int colCount = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

                if (rowCount == 0 || colCount == 0)
                    throw new Exception("Лист пустий");

                if (colCount != 3)
                    throw new Exception("Непраивльне формат листа");

                var headers = new Dictionary<string, int>();

                for (int col = 1; col <= colCount; col++)
                {
                    string header = worksheet.Cell(1, col).GetString().Trim().ToLower();

                    if (header == "піб")
                        headers["fullname"] = col;

                    if (header == "пошта" || header == "email")
                        headers["email"] = col;
                }

                int startRow = 1;

                if (headers.Count > 0)
                    startRow = 2;

                if (!headers.ContainsKey("fullname"))
                    headers["fullname"] = headers.TryGetValue("email", out int value) ? (value == 2 ? 3 : 2) : 2;

                if (!headers.ContainsKey("email"))
                    headers["email"] = headers.TryGetValue("fullname", out int value) ? (value == 2 ? 3 : 2) : 2;

                for (int row = startRow; row <= rowCount; row++)
                {
                    string fullName = worksheet.Cell(row, headers["fullname"]).GetString().Trim();
                    string email = worksheet.Cell(row, headers["email"]).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
                        continue;

                    if (!Validation.Validation.ValidateEmail(email))
                        throw new Exception($"Некоректний email у рядку {row}: {email}");

                    students.Add(new StudentExcelInfo { FullName = fullName, Email = email });
                }
            }

            return students;
        }
    }
}
