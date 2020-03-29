using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    internal class AsyncSlidingWindow<T> : SlidingWindowBase<T>
    {
        private readonly Func<int, IPageStorage<T>> _pageStoreFactory;
        private readonly Func<int, IPageStorage<T>> _placeholderPageStoreFactory;
        private readonly Func<Task<int>> _countFetcher;
        private readonly IScheduler _observeScheduler;
        private IPageStorage<T> _pageStorage;
        
        private readonly SerialDisposable _serialPageStore = new SerialDisposable();

        internal AsyncSlidingWindow(
            int initialSize,
            int initialOffset,
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int, IPageStorage<T>> placeholderPageStoreFactory,
            Func<Task<int>> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable? disposeOnDisposal,
            IScheduler observeScheduler) : base (observeScheduler)
        {
            _pageStoreFactory = pageStoreFactory;
            _placeholderPageStoreFactory = placeholderPageStoreFactory;
            _countFetcher = countFetcher;
            _observeScheduler = observeScheduler;
            CountOfBackedDataSet = 0;
            _pageStorage = _placeholderPageStoreFactory(0);
            InitializationCompleted = ResetInner(initialOffset, initialSize);

            disposeOnDisposal?.AddTo(CompositeDisposable);
            _serialPageStore.AddTo(CompositeDisposable);
            
            observePageFetches
                .ObserveOn(observeScheduler)
                .Subscribe(t =>
                {
                    var ( pageOffset, pageSize, previousPage, page) = t;
                    var start = Math.Max(Offset, pageOffset) - pageOffset;
                    var end = Math.Min(Offset + Size, pageOffset + pageSize) - pageOffset;
                    for (var i = start; i < end; i++)
                    {
                        OnCollectionChangedReplace(page[i], previousPage[i], i - start);
                    }

                    OnIndexerChanged();
                })
                .AddTo(CompositeDisposable);
        }

        protected override int Count => Size;

        private Task ResetInner(int currentOffset, int currentSize)
        {
            return _countFetcher()
                .ToObservable()
                .Do(count =>
                {
                    CountOfBackedDataSet = count;
                    Size = Math.Min(currentSize, count);
                    Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, currentOffset));
                    _pageStorage = _pageStoreFactory(CountOfBackedDataSet).AssignTo(_serialPageStore);
                })
                .ObserveOn(_observeScheduler)
                .Do(count =>
                {
                    OnPropertyChanged(nameof(Count));
                    OnCollectionChangedReset();
                    OnIndexerChanged();
                })
                .ToTask();
        }

        public override void Reset()
        {
            _pageStorage = _placeholderPageStoreFactory(CountOfBackedDataSet).AssignTo(_serialPageStore);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
                {
                    OnPropertyChanged(nameof(Count));
                    OnCollectionChangedReset();
                    OnIndexerChanged();
                });
            ResetInner(Offset, Size);
        }

        protected override T GetItemInner(int index) => _pageStorage[index];

        public override Task InitializationCompleted { get; }
    }
}