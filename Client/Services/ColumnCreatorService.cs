using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Client.Services
{
    public static class ColumnCreatorService
    {
        public static DataGridTextColumn CreateTextColumn(string header, string bindingPath, double widthFactor,
            Style headerStyle, Style cellStyle, Style elementStyle) =>
            new()
            {
                Header = header,
                HeaderStyle = headerStyle,
                Binding = new Binding(bindingPath) { FallbackValue = "Не обрано" },
                Width = new DataGridLength(widthFactor, DataGridLengthUnitType.Star),
                CellStyle = cellStyle,
                ElementStyle = elementStyle
            };

        public static DataGridTextColumn CreateDynamicColumn(double widthFactor, string header,
            string bindingPath, Style headerStyle, Style baseCellStyle, Style elementStyle)
        {
            var cellStyle = new Style(typeof(DataGridCell)) { BasedOn = baseCellStyle };

            var greenTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)1
            };

            greenTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightGreen));

            var redTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)0
            };

            redTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightCoral));

            var yellowTrigger = new DataTrigger
            {
                Binding = new Binding($"{bindingPath}.Approved"),
                Value = (byte)2
            };

            yellowTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightYellow));

            cellStyle.Triggers.Add(greenTrigger);
            cellStyle.Triggers.Add(redTrigger);
            cellStyle.Triggers.Add(yellowTrigger);

            return CreateTextColumn(header, $"{bindingPath}.CodeName", widthFactor, headerStyle, cellStyle, elementStyle);
        }
    }
}
