using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if (DataContext is not AllStudentChoicesViewModel viewModel)
                return;

            var grid = (DataGrid)sender;

            var headerStyle = (Style)FindResource("CenterGridHeaderStyle");
            var materialCellStyle = (Style)FindResource("MaterialDesignDataGridCell");
            var headmanCellStyle = (Style)FindResource("HeadmanCellStyle");
            var centeredCellStyle = (Style)FindResource("CenteredCellStyle");

            grid.Columns.Add(CreateTextColumn("Навчальний рік", "EduYear", 0.2, headerStyle, headmanCellStyle, centeredCellStyle));

            for (int i = 0; i < viewModel.NonparsemesterCount; i++)
            {
                grid.Columns.Add(CreateDynamicColumn(viewModel, $"Непарний {i + 1}", $"Nonparsemester[{i}]",
                    headerStyle, materialCellStyle, centeredCellStyle));
            }

            for (int i = 0; i < viewModel.ParsemesterCount; i++)
            {
                grid.Columns.Add(CreateDynamicColumn(viewModel, $"Парний {i + 1}", $"Parsemester[{i}]",
                    headerStyle, materialCellStyle, centeredCellStyle));
            }
        }

        private DataGridTextColumn CreateTextColumn(string header, string bindingPath, double widthFactor,
            Style headerStyle, Style cellStyle, Style elementStyle)
        {
            return new DataGridTextColumn
            {
                Header = header,
                HeaderStyle = headerStyle,
                Binding = new Binding(bindingPath)
                {
                    FallbackValue = "Не обрано",
                },
                Width = new DataGridLength(widthFactor, DataGridLengthUnitType.Star),
                CellStyle = cellStyle,
                ElementStyle = elementStyle
            };
        }

        private DataGridTextColumn CreateDynamicColumn(AllStudentChoicesViewModel viewModel, string header, string bindingPath,
            Style headerStyle, Style baseCellStyle, Style elementStyle)
        {
            var cellStyle = new Style(typeof(DataGridCell)) { BasedOn = baseCellStyle };

            var greenTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)1
            };

            greenTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGreen));

            var redTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)0
            };

            redTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.LightCoral));

            var yellowTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)2
            };

            yellowTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.LightYellow));

            cellStyle.Triggers.Add(greenTrigger);
            cellStyle.Triggers.Add(redTrigger);
            cellStyle.Triggers.Add(yellowTrigger);

            var widthFactor = 0.8 / (viewModel.NonparsemesterCount + viewModel.ParsemesterCount);

            return CreateTextColumn(header, $"{bindingPath}.CodeName", widthFactor, headerStyle, cellStyle, elementStyle);
        }
    }
}
