using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.View.Views.Decisions
{
    public partial class IndexAccessBehaviorView
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