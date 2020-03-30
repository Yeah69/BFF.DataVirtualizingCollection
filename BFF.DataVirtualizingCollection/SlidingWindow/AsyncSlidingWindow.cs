using System;
using System.Linq;
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
            InitializationCompleted = ResetInner(initialOffset, initialSize, new T[0]);

            disposeOnDisposal?.AddTo(CompositeDisposable);
            _serialPageStore.AddTo(CompositeDisposable);
            
            observePageFetches
                .ObserveOn(observeScheduler)
                .Subscribe(t =>
                {
                    var ( pageOffset, pageSize, previousPage, page) = t;
                    var diff = Offset - pageOffset;
                    var start = Math.Max(0, Offset - pageOffset);
                    var end = Math.Min(start + Size, pageSize);
                    for (var i = start; i < end; i++)
                    {
                        if (i - diff >= Size) break;
                        OnCollectionChangedReplace(page[i], previousPage[i], i - diff);
                    }

                    OnIndexerChanged();
                })
                .AddTo(CompositeDisposable);
        }

        protected override int Count => Size;

        private Task ResetInner(int currentOffset, int currentSize, T[] prev)
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
                    var now = this.Select(x => x).ToArray();
                    OnCollectionChangedReplace(now, prev);
                    OnPropertyChanged(nameof(Offset));
                    OnPropertyChanged(nameof(MaximumOffset));
                    OnPropertyChanged(nameof(Count));
                    OnIndexerChanged();
                })
                .ToTask();
        }

        public override void Reset()
        {
            var prevPrev = this.Select(x => x).ToArray();
            _pageStorage = _placeholderPageStoreFactory(CountOfBackedDataSet).AssignTo(_serialPageStore);
            var prev = this.Select(x => x).ToArray();
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
                {
                    OnCollectionChangedReplace(prev, prevPrev);
                    OnPropertyChanged(nameof(Offset));
                    OnPropertyChanged(nameof(MaximumOffset));
                    OnPropertyChanged(nameof(Count));
                    OnIndexerChanged();
                });
            ResetInner(Offset, Size, prev);
        }

        protected override T GetItemInner(int index) => _pageStorage[index];

        public override Task InitializationCompleted { get; }
    }
}