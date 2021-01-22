using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class AsyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly Func<CancellationToken, Task<int>> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly IScheduler _countBackgroundScheduler;
        private readonly IPageStorage<T> _pageStorage;
        private readonly Subject<Unit> _resetSubject = new Subject<Unit>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private int _count;

        internal AsyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<CancellationToken, Task<int>> countFetcher,
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

            _resetSubject.CompositeDisposalWith(CompositeDisposable);

            _pageStorage = pageStoreFactory(0);
            
            InitializationCompleted = ResetInner(_cancellationTokenSource.Token);

            InitializationCompleted
                .ToObservable()
                .Concat(
                    _resetSubject
                        .SelectMany(async (_, ct) =>
                        {
                            await ResetInner(ct).ConfigureAwait(false);
                            return Unit.Default;
                        }))
                .Subscribe(_ => {}, _cancellationTokenSource.Token);
        }

        public override int Count => _count;

        protected override T GetItemInner(int index) => _pageStorage[index];

        private Task ResetInner(CancellationToken cancellationToken)
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
                .ToTask(cancellationToken);
        }

        public override void Reset() => _resetSubject.OnNext(Unit.Default);

        public override Task InitializationCompleted { get; }

        public override async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await base.DisposeAsync().ConfigureAwait(false);
            await _pageStorage.DisposeAsync().ConfigureAwait(false);
        }
    }
}