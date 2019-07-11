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
        #region Builder

        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher,
                IScheduler subscribeScheduler,
                IScheduler observeScheduler);
        }
        internal interface IBuilderOptional<TItem>
        {
            IDataVirtualizingCollection<TItem> Build();
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IAsyncPageStore<TItem> _pageStore;
            private IScheduler _observeScheduler;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new AsyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher, 
                    _observeScheduler);
            }

            public IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher,
                IScheduler subscribeScheduler,
                IScheduler observeScheduler)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                _observeScheduler = observeScheduler;
                return this;
            }
        }

        #endregion

        private readonly IAsyncPageStore<T> _pageStore;

        private AsyncDataVirtualizingCollection(
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