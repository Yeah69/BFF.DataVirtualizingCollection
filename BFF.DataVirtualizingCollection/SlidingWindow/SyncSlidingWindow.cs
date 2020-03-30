using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;
using BFF.DataVirtualizingCollection.Utilities;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    internal sealed class SyncSlidingWindow<T> : SlidingWindowBase<T>
    {
        private readonly Func<int, IPageStorage<T>> _pageStoreFactory;
        private readonly Func<int> _countFetcher;
        private readonly IScheduler _observeScheduler;
        private readonly SerialDisposable _serialPageStore = new SerialDisposable();
        private IPageStorage<T>? _pageStorage;
        

        internal SyncSlidingWindow(
            int initialSize,
            int initialOffset,
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int> countFetcher,
            IScheduler observeScheduler) : base(observeScheduler)
        {
            _pageStoreFactory = pageStoreFactory;
            _countFetcher = countFetcher;
            _observeScheduler = observeScheduler;
            _serialPageStore.AddTo(CompositeDisposable);
            
            ResetInner(initialSize, initialOffset);
        }

        protected override int Count => Size;

        private void ResetInner(int currentSize, int currentOffset)
        {
            CountOfBackedDataSet = _countFetcher();
            Size = Math.Min(currentSize, CountOfBackedDataSet);
            Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, currentOffset));
            _pageStorage = _pageStoreFactory(CountOfBackedDataSet).AssignTo(_serialPageStore);
        }

        public override void Reset()
        {
            var prev = this.Select(x => x).ToArray();
            ResetInner(Size, Offset);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReplace(this.Select(x => x).ToArray(), prev);
                OnPropertyChanged(nameof(Offset));
                OnPropertyChanged(nameof(MaximumOffset));
                OnPropertyChanged(nameof(Count));
                OnIndexerChanged();
            });
        }

        protected override T GetItemInner(int index)
        {
            if (_pageStorage is null) throw new Exception(ErrorMessage.ImpossibleExceptionMessage); 
            return _pageStorage[index];
        }

        public override Task InitializationCompleted { get; } = Task.CompletedTask;
    }
}