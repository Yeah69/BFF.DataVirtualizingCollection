using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal abstract class DataVirtualizingCollectionBase<T> : IDataVirtualizingCollection<T>
    {
        protected abstract int Count { get; }
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        int ICollection<T>.Count => GetCountInner();

        bool ICollection<T>.IsReadOnly => true;

        protected abstract T GetItemInner(int index);

        public T this[int index]
        {
            get => index >= Count || index < 0 
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : GetItemInner(index);
            set => throw new NotSupportedException();
        }

        private int GetCountInner() => Count;

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int IndexOfInner() => -1;

        public int IndexOf(T item)
        {
            return IndexOfInner();
        }

        private bool ContainsInner() => IndexOfInner() != -1;

        public bool Contains(T item)
        {
            return ContainsInner();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }

        protected void OnCollectionChangedReplace(T newItem, T oldItem, int index)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}