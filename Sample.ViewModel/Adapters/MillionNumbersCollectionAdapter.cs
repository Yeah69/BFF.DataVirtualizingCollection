using BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters
{
    internal interface IMillionNumbersCollectionAdapter : IBackendAccessAdapter<long>
    {
    }

    internal class MillionNumbersCollectionAdapter : IMillionNumbersCollectionAdapter
    {
        private readonly IMillionNumbersBackendAccess _millionNumbersBackendAccess;

        public MillionNumbersCollectionAdapter(IMillionNumbersBackendAccess millionNumbersBackendAccess)
        {
            _millionNumbersBackendAccess = millionNumbersBackendAccess;
        }

        public string Name => _millionNumbersBackendAccess.Name;

        public IBackendAccess<long> BackendAccess => _millionNumbersBackendAccess;

        public BackendAccessKind BackendAccessKind => BackendAccessKind.MillionNumbers;
    }
}