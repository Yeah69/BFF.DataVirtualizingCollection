using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class TaskBasedAsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly IAsyncPageStore<T> _pageStore;

        private readonly Task<int> _countTask;
        private int _count;

        internal TaskBasedAsyncDataVirtualizingCollection(
            IAsyncPageStore<T> pageStore, 
            ITaskBasedCountFetcher countFetcher, 
            IScheduler observeScheduler)
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

            _pageStore.OnCollectionChangedReplace
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3))
                .AddTo(CompositeDisposable);
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