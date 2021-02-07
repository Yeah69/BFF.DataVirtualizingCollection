using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly Func<CancellationToken, int> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly IPageStorage<T> _pageStorage;
        private readonly Subject<Unit> _resetSubject = new();
        
        private int _count;

        internal SyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<CancellationToken, int> countFetcher,
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler)
        : base (observePageFetches, disposeOnDisposal, notificationScheduler)
        {
            _countFetcher = countFetcher;
            _notificationScheduler = notificationScheduler;
            _count = _countFetcher(CancellationToken.None);
            _pageStorage = pageStoreFactory(_count);

            _resetSubject.CompositeDisposalWith(CompositeDisposable);
            
            _resetSubject
                .Subscribe(_ => ResetInner())
                .CompositeDisposalWith(CompositeDisposable);
        }

        public override int Count => _count;

        protected override T GetItemInner(int index)
        {
            return _pageStorage[index];
        }

        private void ResetInner()
        {
            _count = _countFetcher(CancellationToken.None);
            _pageStorage.Reset(_count);
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnPropertyChanged(nameof(Count));
                OnCollectionChangedReset();
                OnIndexerChanged();
            });
        }

        public override void Reset() => _resetSubject.OnNext(Unit.Default);

        public override Task InitializationCompleted { get; } = Task.CompletedTask;

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await _pageStorage.DisposeAsync().ConfigureAwait(false);
        }
    }
}