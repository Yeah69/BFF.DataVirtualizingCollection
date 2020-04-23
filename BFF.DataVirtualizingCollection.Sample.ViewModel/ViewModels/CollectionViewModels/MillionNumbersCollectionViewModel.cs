using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.CollectionViewModels
{
    public interface IMillionNumbersCollectionViewModel : IDataVirtualizingCollectionViewModel<long>
    {
    }

    public class MillionNumbersCollectionViewModel : DataVirtualizingCollectionViewModel<long, long>, IMillionNumbersCollectionViewModel
    {
        public MillionNumbersCollectionViewModel(
            IMillionNumbersBackendAccess backendAccess,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel, 
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel, 
            IFetcherKindViewModel fetcherKindViewModel, 
            IIndexAccessBehaviorViewModel indexAccessBehaviorViewModel,
            IGetSchedulers getSchedulers) 
            : base(
                backendAccess, 
                pageLoadingBehaviorViewModel, 
                pageRemovalBehaviorViewModel, 
                fetcherKindViewModel, 
                indexAccessBehaviorViewModel,
                getSchedulers)
        {
        }

        protected override long[] TransformPage(long[] page)
        {
            return page;
        }

        protected override long TransformPlaceholder(long item)
        {
            return item;
        }
    }
}