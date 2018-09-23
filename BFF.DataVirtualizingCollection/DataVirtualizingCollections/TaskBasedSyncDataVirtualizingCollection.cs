using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class TaskBasedSyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        internal static IDataVirtualizingCollectionBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IDataVirtualizingCollectionBuilderRequired<TItem>
        {
            IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ITaskBasedCountFetcher countFetcher);
        }
        internal interface IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            IDataVirtualizingCollection<TItem> Build();
        }

        internal class Builder<TItem> : IDataVirtualizingCollectionBuilderRequired<TItem>, IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            private ISyncPageStore<TItem> _pageStore;
            private ITaskBasedCountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new TaskBasedSyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher);
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ITaskBasedCountFetcher countFetcher)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                return this;
            }
        }

        private readonly ISyncPageStore<T> _pageStore;
        private readonly Task<int> _countTask;
        private int _count;

        private TaskBasedSyncDataVirtualizingCollection(
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