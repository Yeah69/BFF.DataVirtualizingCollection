using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using MrMeeseeks.Extensions;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly Func<Task<int>> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly IScheduler _countBackgroundScheduler;
        private readonly IPageStorage<T> _pageStorage;
        private readonly Subject<Unit> _resetSubject = new Subject<Unit>();

        private int _count;

        internal AsyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<Task<int>> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler,
            IScheduler countBackgroundScheduler)
        : base(observePageFetches, disposeOnDisposal, notificationScheduler)
        {
            _countFetcher = countFetcher;
            _notificationScheduler = notificationScheduler;
            _countBackgroundScheduler = countBackgroundScheduler;
            _count = 0;

            _resetSubject.AddForDisposalTo(CompositeDisposable);

            _pageStorage = pageStoreFactory(0);
            
            InitializationCompleted = ResetInner();

            InitializationCompleted
                .ToObservable()
                .Concat(
                    _resetSubject
                        .SelectMany(async _ =>
                        {
                            await ResetInner().ConfigureAwait(false);
                            return Unit.Default;
                        }))
                .Subscribe(_ => {})
                .AddTo(CompositeDisposable);
        }

        public override int Count => _count;

        protected override T GetItemInner(int index) => _pageStorage[index];

        private Task ResetInner()
        {
            return Observable.FromAsync(_countFetcher, _countBackgroundScheduler)
                .SelectMany(async count =>
                {
                    _count = count;
                    await _pageStorage.Reset(_count);
                    return Unit.Default;
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

        public override void Reset() => _resetSubject.OnNext(Unit.Default);

        public override Task InitializationCompleted { get; }
    }
}