using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly Func<int, IPageStorage<T>> _pageStoreFactory;
        private readonly Func<int, IPageStorage<T>> _placeholderPageStoreFactory;
        private readonly Func<Task<int>> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly IScheduler _countBackgroundScheduler;
        private readonly SerialDisposable _serialPageStorage = new SerialDisposable();
        private IPageStorage<T> _pageStorage;

        private int _count;

        internal AsyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int, IPageStorage<T>> placeholderPageStoreFactory,
            Func<Task<int>> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler,
            IScheduler countBackgroundScheduler)
        : base(observePageFetches, disposeOnDisposal, notificationScheduler)
        {
            _pageStoreFactory = pageStoreFactory;
            _placeholderPageStoreFactory = placeholderPageStoreFactory;
            _countFetcher = countFetcher;
            _notificationScheduler = notificationScheduler;
            _countBackgroundScheduler = countBackgroundScheduler;
            _count = 0;

            _serialPageStorage.AddTo(CompositeDisposable);

            _pageStorage = _placeholderPageStoreFactory(0);
            InitializationCompleted = ResetInner();
        }

        public override int Count => _count;

        protected override T GetItemInner(int index) => _pageStorage[index];

        private Task ResetInner()
        {
            return Observable.FromAsync(_countFetcher, _countBackgroundScheduler)
                .Do(count =>
                {
                    _count = count;
                    _pageStorage = _pageStoreFactory(_count).AssignTo(_serialPageStorage);
                })
                .ObserveOn(_notificationScheduler)
                .Do(_ =>
                {
                    OnPropertyChanged(nameof(Count));
                    OnCollectionChangedReset();
                    OnIndexerChanged();
                })
                .ToTask();
        }

        public override void Reset()
        {
            _pageStorage = _placeholderPageStoreFactory(_count).AssignTo(_serialPageStorage);
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnPropertyChanged(nameof(Count));
                OnCollectionChangedReset();
                OnIndexerChanged();
            });
            ResetInner();
        }

        public override Task InitializationCompleted { get; }
    }
}