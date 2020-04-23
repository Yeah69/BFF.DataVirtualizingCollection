using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.View.Views.Decisions
{
    public partial class PageLoadingBehaviorView
    {
        public PageLoadingBehaviorView()
        {
            InitializeComponent();
        }

        private void Preloading_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel)
                pageLoadingBehaviorViewModel.PageLoadingBehavior = PageLoadingBehavior.Preloading;
        }

        private void NonPreloading_OnChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel)
                pageLoadingBehaviorViewModel.PageLoadingBehavior = PageLoadingBehavior.NonPreloading;
        }
    }
}