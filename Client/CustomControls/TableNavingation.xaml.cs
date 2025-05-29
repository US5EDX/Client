using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TableNavigation : UserControl
    {
        public static readonly DependencyProperty PreviousPageCommandProperty;
        public static readonly DependencyProperty NextPageCommandProperty;
        public static readonly DependencyProperty CurrentPageProperty;
        public static readonly DependencyProperty TotalPagesProperty;

        public ICommand PreviousPageCommand
        {
            get => (ICommand)GetValue(PreviousPageCommandProperty);
            set => SetValue(PreviousPageCommandProperty, value);
        }

        public ICommand NextPageCommand
        {
            get => (ICommand)GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }

        public int CurrentPage
        {
            get => (int)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public int TotalPages
        {
            get => (int)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }

        static TableNavigation()
        {
            PreviousPageCommandProperty = DependencyProperty
                .Register(nameof(PreviousPageCommand), typeof(ICommand), typeof(TableNavigation));
            NextPageCommandProperty = DependencyProperty
                .Register(nameof(NextPageCommand), typeof(ICommand), typeof(TableNavigation));
            CurrentPageProperty = DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(TableNavigation));
            TotalPagesProperty = DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(TableNavigation));
        }

        public TableNavigation()
        {
            InitializeComponent();
        }
    }
}
