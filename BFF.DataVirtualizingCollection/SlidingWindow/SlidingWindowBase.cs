using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    internal abstract class SlidingWindowBase<T> : VirtualizationBase<T>, ISlidingWindow<T>
    {
        protected int Offset;
        private readonly IScheduler _observeScheduler;

        protected int Size;

        protected int CountOfBackedDataSet;
        internal SlidingWindowBase(
            IScheduler observeScheduler)
        {
            Size = 0;
            Offset = 0;
            _observeScheduler = observeScheduler;
        }

        public void SlideLeft()
        {
            if (Offset <= 0) return;
            Offset--;
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedRemove(GetItemInner(Offset + Size), Size - 1);
                OnCollectionChangedAdd(GetItemInner(Offset), 0);
                OnIndexerChanged();
            });
        }

        public void SlideRight()
        {
            if (CountOfBackedDataSet - Size <= Offset) return;
            Offset++;
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedRemove(GetItemInner(Offset - 1), 0);
                OnCollectionChangedAdd(GetItemInner(Offset + Size - 1), Size - 1);
                OnIndexerChanged();
            });
        }

        public void JumpTo(int index)
        {
            Offset = Math.Max(0, Math.Min(CountOfBackedDataSet - Size, index));
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnCollectionChangedReset();
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
            Size = Math.Min(CountOfBackedDataSet, Size + sizeIncrement);
            if (Offset + Size >= CountOfBackedDataSet)
                JumpTo(CountOfBackedDataSet - Size);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnPropertyChanged(nameof(Count));
            });
        }

        public void DecreaseWindowSizeBy(int sizeIncrement)
        {
            sizeIncrement = Math.Max(0, sizeIncrement);
            Size = Math.Max(0, Size - sizeIncrement);
            _observeScheduler.Schedule(Unit.Default, (_, __) =>
            {
                OnPropertyChanged(nameof(Count));
            });
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return Enumeration().GetEnumerator();

            IEnumerable<T> Enumeration()
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }
        }

        protected override T IndexerInnerGet(int index) =>
            index >= Size || Offset + index >= CountOfBackedDataSet || index < 0
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : GetItemInner(Offset + index);

        protected abstract T GetItemInner(int index);
    }
}