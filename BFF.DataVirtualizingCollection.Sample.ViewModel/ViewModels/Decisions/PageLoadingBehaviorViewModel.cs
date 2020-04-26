using System;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

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
        
        IPageHoldingBehaviorCollectionBuilder<T, TVirtualizationKind> Configure<T, TVirtualizationKind>(
            IPageLoadingBehaviorCollectionBuilder<T, TVirtualizationKind> builder, 
            IBackendAccess<T> backendAccess)
        {
            return PageLoadingBehavior == PageLoadingBehavior.NonPreloading
                ? builder.NonPreloading()
                : builder.Preloading(backendAccess.PreloadingPlaceholderFetch);
        }
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
    }
}