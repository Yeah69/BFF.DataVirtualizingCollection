using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using BFF.DataVirtualizingCollection.DataAccesses;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal abstract class DataVirtualizingCollectionBase<T> : IDataVirtualizingCollection<T>
    {
        protected int Count;
        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected DataVirtualizingCollectionBase(ICountFetcher countFetcher)
        {
            Count = countFetcher.CountFetch();
        }

        int ICollection<T>.Count => GetCountInner();
        int ICollection.Count => GetCountInner();
        public bool IsFixedSize => true;
        bool IList.IsReadOnly => true;
        bool ICollection<T>.IsReadOnly => true;
        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new object();

        protected abstract T GetItemInner(int index);

        public T this[int index]
        {
            get => GetItemInner(index);
            set => throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get => GetItemInner(index);
            set => throw new NotSupportedException();
        }

        protected int GetCountInner() => Count;

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int IndexOfInner() => -1;

        public int IndexOf(object value)
        {
            return IndexOfInner();
        }

        public int IndexOf(T item)
        {
            return IndexOfInner();
        }

        private bool ContainsInner() => IndexOfInner() != -1;

        public bool Contains(T item)
        {
            return ContainsInner();
        }

        public bool Contains(object value)
        {
            return ContainsInner();
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object value)
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

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
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
            CompositeDisposable?.Dispose();
        }

        protected virtual void OnCollectionChangedReplace(T newItem, T oldItem, int index)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}