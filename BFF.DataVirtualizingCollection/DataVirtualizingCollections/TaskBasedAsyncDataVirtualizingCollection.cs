using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class TaskBasedAsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ITaskBasedCountFetcher countFetcher,
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
            private ITaskBasedCountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new TaskBasedAsyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher, 
                    _observeScheduler);
            }

            public IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ITaskBasedCountFetcher countFetcher,
                IScheduler subscribeScheduler,
                IScheduler observeScheduler)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                _observeScheduler = observeScheduler;
                return this;
            }
        }

        private readonly IAsyncPageStore<T> _pageStore;

        private readonly Task<int> _countTask;
        private int _count;

        private TaskBasedAsyncDataVirtualizingCollection(
            IAsyncPageStore<T> pageStore, 
            ITaskBasedCountFetcher countFetcher, 
            IScheduler observeScheduler)
        {
            _pageStore = pageStore;
            _countTask = countFetcher
                .CountFetchAsync()
                .ContinueWith(t => _count = t.Result);

            var disposable = _pageStore.OnCollectionChangedReplace
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3));
            CompositeDisposable.Add(disposable);
            CompositeDisposable.Add(_pageStore);
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