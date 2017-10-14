using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
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
            private IScheduler _subscribeScheduler;
            private IScheduler _observeScheduler;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new AsyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher, 
                    _subscribeScheduler,
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
                _subscribeScheduler = subscribeScheduler;
                return this;
            }
        }

        private readonly IAsyncPageStore<T> _pageStore;

        private AsyncDataVirtualizingCollection(
            IAsyncPageStore<T> pageStore, 
            ICountFetcher countFetcher, 
            IScheduler subscribeScheduler,
            IScheduler observeScheduler)
            : base(countFetcher)
        {
            _pageStore = pageStore;

            var disposable = _pageStore.OnCollectionChangedReplace
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3));
            CompositeDisposable.Add(disposable);
            CompositeDisposable.Add(_pageStore);
        }

        protected override T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
        }
    }
}