using BFF.DataVirtualizingCollection.DataVirtualizingCollection;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions
{
    public enum PageLoadingBehavior
    {
        NonPreloading,
        Preloading
    }

    public interface IPageLoadingBehaviorViewModel
    {
        PageLoadingBehavior PageLoadingBehavior { get; set; }
        IPageHoldingBehaviorCollectionBuilder<T> Configure<T>(IPageLoadingBehaviorCollectionBuilder<T> builder);
    }

    internal class PageLoadingBehaviorViewModel : ObservableObject, IPageLoadingBehaviorViewModel
    {
        private PageLoadingBehavior _pageLoadingBehavior = PageLoadingBehavior.NonPreloading;
        
        public PageLoadingBehavior PageLoadingBehavior
        {
            get => _pageLoadingBehavior;
            set
            {
                if (_pageLoadingBehavior == value) return;
                _pageLoadingBehavior = value;
                OnPropertyChanged();
            }
        }

        public IPageHoldingBehaviorCollectionBuilder<T> Configure<T>(IPageLoadingBehaviorCollectionBuilder<T> builder)
        {
            return _pageLoadingBehavior == PageLoadingBehavior.NonPreloading
                ? builder.NonPreloading()
                : builder.Preloading();
        }
    }
}