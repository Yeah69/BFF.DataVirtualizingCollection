using System;
using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;
using BFF.DataVirtualizingCollection.Sample.Model.Models;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Utility;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters
{
    internal interface IHighWorkloadCollectionAdapter : IBackendAccessAdapter<ISomeWorkloadObjectViewModel>
    {
    }

    internal class HighWorkloadCollectionAdapter : IHighWorkloadCollectionAdapter
    {
        private readonly IHighWorkloadFakeBackendAccess _highWorkloadFakeBackendAccess;
        private readonly Func<ISomeWorkloadObject, ISomeWorkloadObjectViewModel> _someWorkloadObjectViewModelFactory;

        public HighWorkloadCollectionAdapter(
            IHighWorkloadFakeBackendAccess highWorkloadFakeBackendAccess,
            Func<ISomeWorkloadObject, ISomeWorkloadObjectViewModel> someWorkloadObjectViewModelFactory)
        {
            _highWorkloadFakeBackendAccess = highWorkloadFakeBackendAccess;
            _someWorkloadObjectViewModelFactory = someWorkloadObjectViewModelFactory;
            BackendAccess = TransformingBackendAccess<ISomeWorkloadObject, ISomeWorkloadObjectViewModel>
                .CreateTransformingBackendAccess(
                    _highWorkloadFakeBackendAccess,
                    TransformPage,
                    TransformPlaceholder);
        }

        public string Name => _highWorkloadFakeBackendAccess.Name;
        
        public IBackendAccess<ISomeWorkloadObjectViewModel> BackendAccess { get; }

        public BackendAccessKind BackendAccessKind => BackendAccessKind.HighWorkload;

        private ISomeWorkloadObjectViewModel[] TransformPage(ISomeWorkloadObject[] page)
        {
            return page.Select(_someWorkloadObjectViewModelFactory).ToArray();
        }

        private ISomeWorkloadObjectViewModel TransformPlaceholder(ISomeWorkloadObject item)
        {
            return _someWorkloadObjectViewModelFactory(item);
        }
    }
}