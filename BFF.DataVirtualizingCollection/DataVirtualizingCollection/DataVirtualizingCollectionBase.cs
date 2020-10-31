using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MrMeeseeks.Reactive.Extensions;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    internal abstract class DataVirtualizingCollectionBase<T> : VirtualizationBase<T>, IDataVirtualizingCollection<T>
    {
        private int _selectedIndex;
        
        protected DataVirtualizingCollectionBase(
            IObservable<(int Offset, int PageSize, T[] PreviousPage, T[] Page)> observePageFetches,
            IDisposable disposeOnDisposal,
            IScheduler notificationScheduler)
        {
            disposeOnDisposal.CompositeDisposalWith(CompositeDisposable);
            
            observePageFetches
                .ObserveOn(notificationScheduler)
                .Subscribe(t =>
                {
                    var (offset, pageSize, previousPage, page) = t;
                    for (var i = 0; i < pageSize; i++)
                    {
                        OnCollectionChangedReplace(page[i], previousPage[i], i + offset);
                    }
                    OnIndexerChanged();
                })
                .CompositeDisposalWith(CompositeDisposable);
        }
        
        protected override T IndexerInnerGet(int index) =>
            index >= Count || index < 0
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : GetItemInner(index);

        protected abstract T GetItemInner(int index);

        private int _preResetSelectedIndex = -1;
        
        protected override void OnCollectionChangedReset()
        {
            _preResetSelectedIndex = _selectedIndex;
            SelectedIndex = -1; // deselection in order to workaround issue of Selectors
            base.OnCollectionChangedReset();
            SelectedIndex = _preResetSelectedIndex;
        }

        protected override T GetItemForEnumerator(int i) => GetItemInner(i);

        public override int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
    }
}