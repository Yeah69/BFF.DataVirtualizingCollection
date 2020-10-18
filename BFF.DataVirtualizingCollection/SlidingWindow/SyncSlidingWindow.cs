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
        private readonly Func<int> _countFetcher;
        private readonly IScheduler _notificationScheduler;
        private readonly SerialDisposable _serialPageStore = new SerialDisposable();
        private readonly IPageStorage<T> _pageStorage;
        

        internal SyncSlidingWindow(
            int initialSize,
            int initialOffset,
            Func<int, IPageStorage<T>> pageStoreFactory,
            Func<int> countFetcher,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler) 
            : base(disposeOnDisposal, notificationScheduler)
        {
            _countFetcher = countFetcher;
            _notificationScheduler = notificationScheduler;
            _serialPageStore.AddTo(CompositeDisposable);

            _pageStorage = pageStoreFactory(0);
            
            ResetInner(initialSize, initialOffset);
        }

        public override int Count => Size;

        private void ResetInner(int currentSize, int currentOffset)
        {
            CountOfBackedDataSet = _countFetcher();
            Size = Math.Min(currentSize, CountOfBackedDataSet);
            Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, currentOffset));
            _pageStorage.Reset(CountOfBackedDataSet);
        }

        public override void Reset()
        {
            var prev = this.Select(x => x).ToArray();
            ResetInner(Size, Offset);
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
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