using BFF.DataVirtualizingCollection.DataVirtualizingCollection;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions
{
    public enum PageRemovalBehavior
    {
        LeastRecentlyUsed,
        Hoarding
    }

    public interface IPageRemovalBehaviorViewModel
    {
        PageRemovalBehavior PageRemovalBehavior { get; set; }
        IFetchersKindCollectionBuilder<T> Configure<T>(IPageHoldingBehaviorCollectionBuilder<T> builder);
    }

    internal class PageRemovalBehaviorViewModel : ObservableObject, IPageRemovalBehaviorViewModel
    {
        private PageRemovalBehavior _pageRemovalBehavior = PageRemovalBehavior.LeastRecentlyUsed;
        private int _leastRecentlyUsedPageLimit = 10;
        private int _leastRecentlyUsedRemovalCount = 1;
        
        public PageRemovalBehavior PageRemovalBehavior
        {
            get => _pageRemovalBehavior;
            set
            {
                if (_pageRemovalBehavior == value) return;
                _pageRemovalBehavior = value;
                OnPropertyChanged();
            }
        }

        public int LeastRecentlyUsedPageLimit
        {
            get => _leastRecentlyUsedPageLimit;
            set
            {
                if (_leastRecentlyUsedPageLimit == value) return;
                _leastRecentlyUsedPageLimit = value;
                OnPropertyChanged();
            }
        }

        public int LeastRecentlyUsedRemovalCount
        {
            get => _leastRecentlyUsedRemovalCount;
            set
            {
                if (_leastRecentlyUsedRemovalCount == value) return;
                _leastRecentlyUsedRemovalCount = value;
                OnPropertyChanged();
            }
        }

        public IFetchersKindCollectionBuilder<T> Configure<T>(IPageHoldingBehaviorCollectionBuilder<T> builder)
        {
            return _pageRemovalBehavior == PageRemovalBehavior.LeastRecentlyUsed
                ? builder.LeastRecentlyUsed(_leastRecentlyUsedPageLimit, _leastRecentlyUsedRemovalCount)
                : builder.Hoarding();
        }
    }
}