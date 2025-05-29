using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for FrameStateContainer.xaml
    /// </summary>
    public partial class FrameStateContainer : UserControl
    {
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(FrameStateContainer));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty HasErrorMessageProperty =
            DependencyProperty.Register(nameof(HasErrorMessage), typeof(bool), typeof(FrameStateContainer));

        public bool HasErrorMessage
        {
            get => (bool)GetValue(HasErrorMessageProperty);
            set => SetValue(HasErrorMessageProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(FrameStateContainer));

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        public static readonly DependencyProperty RetryCommandProperty =
            DependencyProperty.Register(nameof(RetryCommand), typeof(ICommand), typeof(FrameStateContainer));

        public ICommand RetryCommand
        {
            get => (ICommand)GetValue(RetryCommandProperty);
            set => SetValue(RetryCommandProperty, value);
        }

        public static readonly DependencyProperty CurrentFrameProperty =
            DependencyProperty.Register(nameof(CurrentFrame), typeof(object), typeof(FrameStateContainer));

        public object CurrentFrame
        {
            get => GetValue(CurrentFrameProperty);
            set => SetValue(CurrentFrameProperty, value);
        }

        public FrameStateContainer()
        {
            InitializeComponent();
        }

        private void Frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode is not NavigationMode.New or NavigationMode.Refresh)
                e.Cancel = true;
        }
    }
}
