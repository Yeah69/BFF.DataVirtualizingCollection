using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private IPageStorage<T>? _pageStorage;

        private int _count;

        internal AsyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<Task<int>> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable? disposeOnDisposal,
            IScheduler observeScheduler)
        {
            _count = 0;
            InitializationCompleted = countFetcher()
                .ToObservable()
                .ObserveOn(observeScheduler)
                .Do(count =>
                {
                    _count = count;
                    _pageStorage = pageStoreFactory(_count).AddTo(CompositeDisposable);
                    OnPropertyChanged(nameof(Count));
                })
                .ToTask();

            disposeOnDisposal?.AddTo(CompositeDisposable);
            
            observePageFetches
                .ObserveOn(observeScheduler)
                .Subscribe(t =>
                {
                    var (offset, pageSize, previousPage, page) = t;
                    for (var i = 0; i < pageSize; i++)
                    {
                        OnCollectionChangedReplace(page[i], previousPage[i], i + offset);
                    }
                })
                .AddTo(CompositeDisposable);
        }

        protected override int Count => _count;

        protected override T GetItemInner(int index)
        {
            InitializationCompleted.Wait();
            return _pageStorage is null
                ? throw new Exception("Impossible")
                : _pageStorage[index];
        }

        public override Task InitializationCompleted { get; }
    }
}