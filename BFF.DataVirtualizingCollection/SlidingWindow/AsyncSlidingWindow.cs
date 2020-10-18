using System;
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
        private readonly Func<Task<int>> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly IScheduler _countBackgroundScheduler;
        private readonly IPageStorage<T> _pageStorage;
        
        private readonly SerialDisposable _serialPageStore = new SerialDisposable();

        internal AsyncSlidingWindow(
            int initialSize,
            int initialOffset,
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<Task<int>> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler,
            IScheduler countBackgroundScheduler) 
            : base (disposeOnDisposal, notificationScheduler)
        {
            _countFetcher = countFetcher;
            _notificationScheduler = notificationScheduler;
            _countBackgroundScheduler = countBackgroundScheduler;
            CountOfBackedDataSet = 0;
            _pageStorage = pageStoreFactory(0);
            InitializationCompleted = ResetInner(initialOffset, initialSize);

            _serialPageStore.AddTo(CompositeDisposable);
            
            observePageFetches
                .ObserveOn(notificationScheduler)
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

        public override int Count => Size;

        private Task ResetInner(int currentOffset, int currentSize)
        {
            return Observable.FromAsync(_countFetcher, _countBackgroundScheduler)
                .Do(count =>
                {
                    CountOfBackedDataSet = count;
                    Size = Math.Min(currentSize, count);
                    Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, currentOffset));
                    _pageStorage.Reset(CountOfBackedDataSet);
                })
                .ObserveOn(_notificationScheduler)
                .Do(count =>
                {
                    OnCollectionChangedReset();
                    OnPropertyChanged(nameof(Offset));
                    OnPropertyChanged(nameof(MaximumOffset));
                    OnPropertyChanged(nameof(Count));
                    OnIndexerChanged();
                })
                .ToTask();
        }

        public override void Reset()
        {
            ResetInner(Offset, Size);
        }

        protected override T GetItemInner(int index) => _pageStorage[index];

        public override Task InitializationCompleted { get; }
    }
}