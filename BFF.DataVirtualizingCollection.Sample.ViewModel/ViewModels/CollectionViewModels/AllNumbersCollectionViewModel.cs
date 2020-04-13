using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.CollectionViewModels
{
    public interface IAllNumbersCollectionViewModel : IDataVirtualizingCollectionViewModel<int>
    {
    }

    public class AllNumbersCollectionViewModel : DataVirtualizingCollectionViewModel<int, int>, IAllNumbersCollectionViewModel
    {
        public AllNumbersCollectionViewModel(
            IAllNumbersFakeBackendAccess backendAccess,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel, 
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel, 
            IFetcherKindViewModel fetcherKindViewModel, 
            IIndexAccessBehaviorViewModel indexAccessBehaviorViewModel) 
            : base(
                backendAccess, 
                pageLoadingBehaviorViewModel, 
                pageRemovalBehaviorViewModel, 
                fetcherKindViewModel, 
                indexAccessBehaviorViewModel)
        {
        }

        protected override int[] TransformPage(int[] page)
        {
            return page;
        }

        protected override int TransformPlaceholder(int item)
        {
            return item;
        }
    }
}