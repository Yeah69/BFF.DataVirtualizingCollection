using System.Windows;
using System.Windows.Controls;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.View.Views.Decisions
{
    public partial class IndexAccessBehaviorView : UserControl
    {
        public IndexAccessBehaviorView()
        {
            InitializeComponent();
        }

        private void Synchronous_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IIndexAccessBehaviorViewModel indexAccessBehaviorViewModel)
                indexAccessBehaviorViewModel.IndexAccessBehavior = IndexAccessBehavior.Synchronous;
        }

        private void Asynchronous_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IIndexAccessBehaviorViewModel indexAccessBehaviorViewModel)
                indexAccessBehaviorViewModel.IndexAccessBehavior = IndexAccessBehavior.Asynchronous;
        }
    }
}