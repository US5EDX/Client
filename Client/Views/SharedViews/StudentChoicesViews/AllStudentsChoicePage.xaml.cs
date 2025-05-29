using Client.Services;
using Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for AllStudentsChoicePage.xaml
    /// </summary>
    public partial class AllStudentsChoicePage : Page
    {
        public AllStudentsChoicePage()
        {
            InitializeComponent();
        }

        private void StudentsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not AllStudentChoicesViewModel viewModel || sender is not DataGrid grid) return;

            var headerStyle = (Style)FindResource("CenterGridHeaderStyle");
            var materialCellStyle = (Style)FindResource("MaterialDesignDataGridCell");
            var centeredCellStyle = (Style)FindResource("CenteredCellStyle");

            grid.Columns.Add(ColumnCreatorService
                .CreateTextColumn("Навчальний рік", "EduYear", 0.2, headerStyle, materialCellStyle, centeredCellStyle));

            double widthFactor = 0.8 / (viewModel.NonparsemesterCount + viewModel.ParsemesterCount);

            for (int i = 0; i < viewModel.NonparsemesterCount; i++)
                grid.Columns.Add(ColumnCreatorService
                    .CreateDynamicColumn(widthFactor, $"Осінній {i + 1}", $"Nonparsemester[{i}]",
                    headerStyle, materialCellStyle, centeredCellStyle));

            for (int i = 0; i < viewModel.ParsemesterCount; i++)
                grid.Columns.Add(ColumnCreatorService
                    .CreateDynamicColumn(widthFactor, $"Весняний {i + 1}", $"Parsemester[{i}]",
                    headerStyle, materialCellStyle, centeredCellStyle));
        }
    }
}
