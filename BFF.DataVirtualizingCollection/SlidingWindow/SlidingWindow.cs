using System;
using System.Collections.Specialized;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    internal sealed class SlidingWindow<T> : VirtualizationBase<T>, ISlidingWindow<T>
    {
        private readonly IDataVirtualizingCollection<T> _dataVirtualizingCollection;
        private readonly IScheduler _notificationScheduler;

        private int _size;
        
        internal SlidingWindow(
            int offset,
            int windowSize,
            IDataVirtualizingCollection<T> dataVirtualizingCollection,
            IScheduler notificationScheduler)
        {
            _size = 0;
            Offset = 0;
            _dataVirtualizingCollection = dataVirtualizingCollection;
            _notificationScheduler = notificationScheduler;

            Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler => handler.Invoke,
                    h => _dataVirtualizingCollection.CollectionChanged += h,
                    h => _dataVirtualizingCollection.CollectionChanged -= h)
                .ObserveOn(notificationScheduler)
                .Subscribe(e =>
                {
                    switch (e.EventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Replace:
                            var index = e.EventArgs.NewStartingIndex - Offset;
                            if (index > _size) return;
                            OnCollectionChangedReplace((T) e.EventArgs.NewItems[0], (T) e.EventArgs.OldItems[0], index);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            OnCollectionChangedReset();
                            break;
                        default:
                            throw new Exception("Something unexpected happened.");
                    }
                });
            
            _dataVirtualizingCollection
                .ObservePropertyChanged(nameof(IDataVirtualizingCollection.Count))
                .ObserveOn(notificationScheduler)
                .Subscribe(_ =>
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(MaximumOffset));
                })
                .CompositeDisposalWith(CompositeDisposable);
            
            _dataVirtualizingCollection
                .ObservePropertyChanged("Item[]")
                .ObserveOn(notificationScheduler)
                .Subscribe(_ => OnIndexerChanged())
                .CompositeDisposalWith(CompositeDisposable);

            InitializationCompleted = dataVirtualizingCollection
                .InitializationCompleted
                .ToObservable()
                .ObserveOn(notificationScheduler)
                .Do(_ =>
                {
                    _size = windowSize;
                    JumpTo(offset);
                })
                .ToTask();
        }
        
        public int Offset { get; private set; }

        public int MaximumOffset => _dataVirtualizingCollection.Count - _size;
        
        public override int SelectedIndex { get; set; }

        public void SlideLeft()
        {
            if (Offset <= 0) return;
            Offset--;
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void SlideRight()
        {
            if (MaximumOffset <= Offset) return;
            Offset++;
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void JumpTo(int index)
        {
            Offset = Math.Max(0, Math.Min(_dataVirtualizingCollection.Count - _size, index));
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
                OnPropertyChanged(nameof(Offset));
                OnIndexerChanged();
            });
        }

        public void IncreaseWindowSize()
        {
            IncreaseWindowSizeBy(1);
        }

        public void DecreaseWindowSize()
        {
            DecreaseWindowSizeBy(1);
        }

        public void IncreaseWindowSizeBy(int sizeIncrement)
        {
            sizeIncrement = Math.Max(0, sizeIncrement);
            _size = Math.Min(_dataVirtualizingCollection.Count, _size + sizeIncrement);
            if (Offset > MaximumOffset)
                Offset = MaximumOffset;
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
                OnPropertyChanged(nameof(Offset));
                OnPropertyChanged(nameof(MaximumOffset));
                OnPropertyChanged(nameof(Count));
                OnIndexerChanged();
            });
        }

        public void DecreaseWindowSizeBy(int sizeIncrement)
        {
            sizeIncrement = Math.Max(0, sizeIncrement);
            _size = Math.Max(0, _size - sizeIncrement);
            _notificationScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
                OnPropertyChanged(nameof(Offset));
                OnPropertyChanged(nameof(MaximumOffset));
                OnPropertyChanged(nameof(Count));
                OnIndexerChanged();
            });
        }

        public void SetWindowSizeTo(int size)
        {
            if (size < 0) throw new ArgumentException("Given size may not be below zero.", nameof(size));
            if (size == _size) return;
            var diff = size - _size;
            if (diff < 0) DecreaseWindowSizeBy(-diff);
            else IncreaseWindowSizeBy(diff);
        }

        protected override T GetItemForEnumerator(int i) => this[i];

        public override int Count => _size;

        protected override T IndexerInnerGet(int index) =>
            _dataVirtualizingCollection[Offset + index];

        public override void Reset() => _dataVirtualizingCollection.Reset();

        public override Task InitializationCompleted { get; }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await _dataVirtualizingCollection.DisposeAsync().ConfigureAwait(false);
        }
    }
}