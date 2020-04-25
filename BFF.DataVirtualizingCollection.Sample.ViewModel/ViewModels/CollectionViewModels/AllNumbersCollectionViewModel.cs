using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
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
            IFetcherKindViewModelInternal fetcherKindViewModel, 
            IGetSchedulers getSchedulers) 
            : base(
                backendAccess, 
                pageLoadingBehaviorViewModel, 
                pageRemovalBehaviorViewModel, 
                fetcherKindViewModel, 
                getSchedulers)
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