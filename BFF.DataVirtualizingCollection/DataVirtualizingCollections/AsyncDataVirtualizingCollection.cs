using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;
using JetBrains.Annotations;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private IPageStorage<T> _pageStorage;

        private int _count;

        internal AsyncDataVirtualizingCollection(
            [NotNull] Func<int, IPageStorage<T>> pageStoreFactory,
            [NotNull] Func<Task<int>> countFetcher,
            [NotNull] IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            [CanBeNull] IDisposable disposeOnDisposal,
            [NotNull] IScheduler observeScheduler)
        {
            pageStoreFactory = pageStoreFactory ?? throw new ArgumentNullException(nameof(pageStoreFactory));
            countFetcher = countFetcher ?? throw new ArgumentNullException(nameof(countFetcher));
            observePageFetches = observePageFetches ?? throw new ArgumentNullException(nameof(observePageFetches));
            observeScheduler = observeScheduler ?? throw new ArgumentNullException(nameof(observeScheduler));

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
            return _pageStorage[index];
        }

        public override Task InitializationCompleted { get; }
    }
}