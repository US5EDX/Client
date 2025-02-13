using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        public static readonly DependencyProperty CanDeepSearchProperty;
        public static readonly DependencyProperty IsDeepSearchVisibleProperty;
        public static readonly DependencyProperty DeepSearchCommandProperty;

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

        public bool CanDeepSearch
        {
            get => (bool)GetValue(CanDeepSearchProperty);
            set => SetValue(CanDeepSearchProperty, value);
        }

        public bool IsDeepSearchVisible
        {
            get => (bool)GetValue(IsDeepSearchVisibleProperty);
            set => SetValue(IsDeepSearchVisibleProperty, value);
        }

        public Func<object, string, bool> Filter
        {
            get => (Func<object, string, bool>)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public IAsyncRelayCommand DeepSearchCommand
        {
            get => (IAsyncRelayCommand)GetValue(DeepSearchCommandProperty);
            set => SetValue(DeepSearchCommandProperty, value);
        }

        static SearchBlock()
        {
            CollectionProperty = DependencyProperty.Register("Collection", typeof(object), typeof(SearchBlock));
            CanSearchProperty = DependencyProperty.Register("CanSearch", typeof(bool), typeof(SearchBlock));
            FilterProperty = DependencyProperty.Register("Filter", typeof(Func<object, string, bool>), typeof(SearchBlock));
            CanDeepSearchProperty = DependencyProperty.Register("CanDeepSearch", typeof(bool), typeof(SearchBlock));
            DeepSearchCommandProperty = DependencyProperty.Register("DeepSearchCommand", typeof(IAsyncRelayCommand), typeof(SearchBlock));
            IsDeepSearchVisibleProperty = DependencyProperty.Register("IsDeepSearchVisible", typeof(bool), typeof(SearchBlock));
        }

        public SearchBlock()
        {
            InitializeComponent();
            searchButton.Click += OnSearchClick;
            deepSearchButton.Click += OnDeepSearchClickAsync;

            var binding = new Binding("CanSearch")
            {
                Source = this,
                Mode = BindingMode.OneWay
            };

            searchButton.SetBinding(Button.IsEnabledProperty, binding);

            binding = new Binding("CanDeepSearch")
            {
                Source = this,
                Mode = BindingMode.OneWay
            };

            deepSearchButton.SetBinding(Button.IsEnabledProperty, binding);

            if (!CanDeepSearch)
                return;

            binding = new Binding("IsDeepSearchVisible")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            };

            deepSearchButton.SetBinding(Button.VisibilityProperty, binding);
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (Collection == null)
                return;

            var collectionView = CollectionViewSource.GetDefaultView(Collection);
            collectionView.Filter = null;
            collectionView.Refresh();

            if (searchText.Text.Length > 2)
            {
                collectionView.Filter = FilterObjects;
                collectionView.Refresh();
            }

            if (!CanDeepSearch)
                return;

            if (!CollectionViewSource.GetDefaultView(Collection).IsEmpty)
                IsDeepSearchVisible = false;
            else
                IsDeepSearchVisible = true;
        }

        private bool FilterObjects(object obj)
        {
            return Filter(obj, searchText.Text);
        }

        private async void OnDeepSearchClickAsync(object sender, RoutedEventArgs e)
        {
            if (DeepSearchCommand is null)
                return;

            await DeepSearchCommand.ExecuteAsync(searchText.Text);
        }
    }
}
