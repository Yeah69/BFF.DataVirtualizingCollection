using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters
{
    internal interface IAllNumbersCollectionAdapter : IBackendAccessAdapter<int>
    {
    }

    internal class AllNumbersCollectionAdapter :  IAllNumbersCollectionAdapter
    {
        private readonly IAllNumbersFakeBackendAccess _allNumbersFakeBackendAccess;

        public AllNumbersCollectionAdapter(IAllNumbersFakeBackendAccess allNumbersFakeBackendAccess)
        {
            _allNumbersFakeBackendAccess = allNumbersFakeBackendAccess;
            
        }

        public string Name => _allNumbersFakeBackendAccess.Name;

        public IBackendAccess<int> BackendAccess => _allNumbersFakeBackendAccess;

        public BackendAccessKind BackendAccessKind => BackendAccessKind.AllNumbers;
    }
}