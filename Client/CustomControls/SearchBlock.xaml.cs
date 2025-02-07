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

namespace Client.CustomControls
{
    /// <summary>
    /// Interaction logic for SearchBlock.xaml
    /// </summary>
    public partial class SearchBlock : UserControl
    {
        public static readonly DependencyProperty CollectionProperty;
        public static readonly DependencyProperty CanSearchProperty;
        public static readonly DependencyProperty FilterProperty;

        public object Collection
        {
            get => GetValue(CollectionProperty);
            set => SetValue(CollectionProperty, value);
        }

        public bool CanSearch
        {
            get => (bool)GetValue(CanSearchProperty);
            set => SetValue(CanSearchProperty, value);
        }

        public Func<object, string, bool> Filter
        {
            get => (Func<object, string, bool>)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        static SearchBlock()
        {
            CollectionProperty = DependencyProperty.Register("Collection", typeof(object), typeof(SearchBlock));
            CanSearchProperty = DependencyProperty.Register("CanSearch", typeof(bool), typeof(SearchBlock));
            FilterProperty = DependencyProperty.Register("Filter", typeof(Func<object, string, bool>), typeof(SearchBlock));
        }

        public SearchBlock()
        {
            InitializeComponent();
            searchButton.Click += OnSearchClick;

            var binding = new Binding("CanSearch")
            {
                Source = this,
                Mode = BindingMode.OneWay
            };

            searchButton.SetBinding(Button.IsEnabledProperty, binding);
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            if (Collection == null)
                return;

            CollectionViewSource.GetDefaultView(Collection).Filter = null;
            CollectionViewSource.GetDefaultView(Collection).Refresh();

            if (searchText.Text.Length > 2)
                CollectionViewSource.GetDefaultView(Collection).Filter = FilterObjects;
        }

        private bool FilterObjects(object obj)
        {
            return Filter(obj, searchText.Text);
        }
    }
}
