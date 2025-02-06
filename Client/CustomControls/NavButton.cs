using System.Windows;
using System.Windows.Controls;

namespace Client.CustomControls
{
    public class NavButton : RadioButton
    {
        static NavButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavButton), new FrameworkPropertyMetadata(typeof(NavButton)));
        }

        public string ContentInfo
        {
            get { return (string)GetValue(ContentInfoProperty); }
            set { SetValue(ContentInfoProperty, value); }
        }

        public static readonly DependencyProperty ContentInfoProperty =
            DependencyProperty.Register("ContentInfo", typeof(string), typeof(NavButton), new PropertyMetadata(null));
    }
}
