using Client.Converters;
using Client.Models;
using Client.ViewModels.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Client.ViewModels
{
    public partial class DisciplinePreviewViewModel : ObservableRecipient, IPageViewModel
    {
        public List<PreviewPair> Pairs { get; init; }

        public IRelayCommand CloseCommand { get; set; }

        public DisciplinePreviewViewModel(IRelayCommand closeCommand, DisciplineFullInfo discipline)
        {
            CloseCommand = closeCommand;

            Pairs =
            [
                new PreviewPair("Код дисципліни", discipline.DisciplineCode),
                new PreviewPair("Назва дисципліни", discipline.DisciplineName),
                new PreviewPair("Тип каталогу",
                    (string)new CatalogTypeToCatalogNameConverter().Convert(discipline.CatalogType, null, null, null)),
                new PreviewPair("Факультет", discipline.Faculty.FacultyName),
                new PreviewPair("Спеціальність", discipline.Specialty?.SpecialtyName ?? "Не вказано"),
                new PreviewPair("Рівень ВО",
                    (string)new EduLevelToEduLevelNameConverter().Convert(discipline.EduLevel, null, null, null)),
                new PreviewPair("Курс", $"Для {discipline.Course} курсу"),
                new PreviewPair("Семестр",
                    (string)new SemesterToSemesterNameConverter().Convert(discipline.Semester, null, null, null)),
                new PreviewPair("Розрахована на рік?", discipline.IsYearLong ? "Так" : "Ні"),
                new PreviewPair("Пререквізити", discipline.Prerequisites),
                new PreviewPair("Чому це цікаво / треба вивчати", discipline.Interest),
                new PreviewPair("Максимальна кількість здобувачів",
                    discipline.MaxCount == 0 ? "Необмежено" : discipline.MaxCount.ToString()),
                new PreviewPair("Мінімальна кількість здобувачів",
                    discipline.MinCount == 0 ? "Не вказано" : discipline.MinCount.ToString()),
                new PreviewPair("Посилання на документ з повною інформацією", discipline.Url),
                new PreviewPair("Навчальний рік", discipline.Holding.ToString()),
            ];
        }

        [RelayCommand]
        private void OpenUrl()
        {
            Process.Start(new ProcessStartInfo(
                Pairs.First(p => p.Name == "Посилання на документ з повною інформацією").Description)
            { UseShellExecute = true });
        }
    }
}
