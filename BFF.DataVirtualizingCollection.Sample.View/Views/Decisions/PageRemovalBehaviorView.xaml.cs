using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.View.Views.Decisions
{
    public partial class PageRemovalBehaviorView
    {
        public PageRemovalBehaviorView() => InitializeComponent();

        private void Hoarding_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IPageRemovalBehaviorViewModel pageLoadingBehaviorViewModel)
                pageLoadingBehaviorViewModel.PageRemovalBehavior = PageRemovalBehavior.Hoarding;
        }

        private void LeastRecentlyUsed_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IPageRemovalBehaviorViewModel pageLoadingBehaviorViewModel)
                pageLoadingBehaviorViewModel.PageRemovalBehavior = PageRemovalBehavior.LeastRecentlyUsed;
        }
    }
}