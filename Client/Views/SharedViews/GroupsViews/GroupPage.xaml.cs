using Client.Services;
using Client.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Client.Views
{
    /// <summary>
    /// Interaction logic for GroupPage.xaml
    /// </summary>
    public partial class GroupPage : Page
    {
        public GroupPage()
        {
            InitializeComponent();
        }

        private void StudentsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not GroupPageViewModel viewModel || sender is not DataGrid grid) return;

            var headerStyle = (Style)FindResource("CenterGridHeaderStyle");
            var materialCellStyle = (Style)FindResource("MaterialDesignDataGridCell");
            var headmanCellStyle = (Style)FindResource("HeadmanCellStyle");
            var centeredCellStyle = (Style)FindResource("CenteredCellStyle");

            grid.Columns.Add(ColumnCreatorService
                .CreateTextColumn("Пошта", "Email", 0.15, headerStyle, headmanCellStyle, centeredCellStyle));
            grid.Columns.Add(ColumnCreatorService
                .CreateTextColumn("ПІБ", "FullName", 0.15, headerStyle, headmanCellStyle, centeredCellStyle));

            double widthFactor = 0.7 / (viewModel.NonparsemesterCount + viewModel.ParsemesterCount);

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
