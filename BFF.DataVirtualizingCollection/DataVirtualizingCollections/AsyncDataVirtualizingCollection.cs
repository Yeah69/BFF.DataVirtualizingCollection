using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly IAsyncPageStore<T> _pageStore;

        internal AsyncDataVirtualizingCollection(
            IAsyncPageStore<T> pageStore, 
            ICountFetcher countFetcher, 
            IScheduler observeScheduler)
        {
            _pageStore = pageStore.AddTo(CompositeDisposable);

            Count = countFetcher.CountFetch();

            _pageStore.Count = Count;

            _pageStore.OnCollectionChangedReplace
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3))
                .AddTo(CompositeDisposable);
        }

        protected override int Count { get; }

        protected override T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
        }
    }
}