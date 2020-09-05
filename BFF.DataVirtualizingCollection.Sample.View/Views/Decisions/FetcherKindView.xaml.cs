using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.View.Views.Decisions
{
    public partial class FetcherKindView
    {
        public FetcherKindView() => InitializeComponent();

        private void NonTaskBased_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IFetcherKindViewModel fetcherKindViewModel)
                fetcherKindViewModel.FetcherKind = FetcherKind.NonTaskBased;
        }

        private void TaskBased_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IFetcherKindViewModel fetcherKindViewModel)
                fetcherKindViewModel.FetcherKind = FetcherKind.TaskBased;
        }
    }
}