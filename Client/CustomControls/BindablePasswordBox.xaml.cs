using System.Windows;
using System.Windows.Controls;

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        public static readonly DependencyProperty PasswordProperty;

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        static BindablePasswordBox()
        {
            PasswordProperty = DependencyProperty.Register("Password", typeof(string),
                typeof(BindablePasswordBox), new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
            txtPassword.PasswordChanged += OnPasswordChanged;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = txtPassword.Password;
        }

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BindablePasswordBox passwordBox && e.NewValue is string newPassword)
            {
                if (string.IsNullOrEmpty(newPassword))
                {
                    passwordBox.txtPassword.Password = string.Empty;
                }
            }
        }
    }
}
