using System;
using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.Model.Models;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.Decisions;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels.CollectionViewModels
{
    public interface IHighWorkloadCollectionViewModel : IDataVirtualizingCollectionViewModel<ISomeWorkloadObjectViewModel>
    {
    }

    public class HighWorkloadCollectionViewModel : DataVirtualizingCollectionViewModel<ISomeWorkloadObject, ISomeWorkloadObjectViewModel>, IHighWorkloadCollectionViewModel
    {
        private readonly Func<ISomeWorkloadObject, ISomeWorkloadObjectViewModel> _someWorkloadObjectViewModelFactory;

        public HighWorkloadCollectionViewModel(
            IHighWorkloadFakeBackendAccess backendAccess,
            IPageLoadingBehaviorViewModel pageLoadingBehaviorViewModel, 
            IPageRemovalBehaviorViewModel pageRemovalBehaviorViewModel, 
            IFetcherKindViewModelInternal fetcherKindViewModel, 
            Func<ISomeWorkloadObject, ISomeWorkloadObjectViewModel> someWorkloadObjectViewModelFactory,
            IGetSchedulers getSchedulers) 
            : base(
                backendAccess, 
                pageLoadingBehaviorViewModel, 
                pageRemovalBehaviorViewModel, 
                fetcherKindViewModel, 
                getSchedulers)
        {
            _someWorkloadObjectViewModelFactory = someWorkloadObjectViewModelFactory;
        }

        protected override ISomeWorkloadObjectViewModel[] TransformPage(ISomeWorkloadObject[] page)
        {
            return page.Select(_someWorkloadObjectViewModelFactory).ToArray();
        }

        protected override ISomeWorkloadObjectViewModel TransformPlaceholder(ISomeWorkloadObject item)
        {
            return _someWorkloadObjectViewModelFactory(item);
        }
    }
}