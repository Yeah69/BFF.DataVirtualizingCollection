using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class TaskBasedSyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly ISyncPageStore<T> _pageStore;
        private readonly Task<int> _countTask;
        private int _count;

        internal TaskBasedSyncDataVirtualizingCollection(
            ISyncPageStore<T> pageStore,
            ITaskBasedCountFetcher countFetcher)
        {
            _pageStore = pageStore.AddTo(CompositeDisposable);
            _countTask = countFetcher
                .CountFetchAsync()
                .ContinueWith(t =>
                {
                    _count = t.Result;
                    _pageStore.Count = _count;
                    return t.Result;
                });
        }

        protected override int Count
        {
            get
            {
                _countTask.Wait();
                return _count;
            }
        }

        protected override T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
        }
    }
}