using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal sealed class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly Func<int, IPageStorage<T>> _pageStoreFactory;
        private readonly Func<int> _countFetcher;
        private readonly IScheduler _observeScheduler;
        private IPageStorage<T> _pageStorage;
        private readonly SerialDisposable _serialPageStorage = new SerialDisposable();
        private int _count;

        internal SyncDataVirtualizingCollection(
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int> countFetcher,
            IScheduler observeScheduler)
        {
            _pageStoreFactory = pageStoreFactory;
            _countFetcher = countFetcher;
            _observeScheduler = observeScheduler;
            _count = _countFetcher();
            _serialPageStorage.AddTo(CompositeDisposable);
            _pageStorage = _pageStoreFactory(_count).AssignTo(_serialPageStorage);
        }

        protected override int Count => _count;

        protected override T GetItemInner(int index)
        {
            return _pageStorage[index];
        }

        public override void Reset()
        {
            _count = _countFetcher();
            _pageStorage = _pageStoreFactory(_count).AssignTo(_serialPageStorage);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnPropertyChanged(nameof(Count));
                OnCollectionChangedReset();
                OnIndexerChanged();
            });
        }

        public override Task InitializationCompleted { get; } = Task.CompletedTask;
    }
}