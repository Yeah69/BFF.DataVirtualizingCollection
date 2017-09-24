using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class SyncDataVirtualizingCollection<T> : IDataVirtualizingCollection<T>
    {
        internal static IDataVirtualizingCollectionBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IDataVirtualizingCollectionBuilderRequired<TItem>
        {
            IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher);
        }
        internal interface IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            IDataVirtualizingCollection<TItem> Build();
        }

        internal class Builder<TItem> : IDataVirtualizingCollectionBuilderRequired<TItem>, IDataVirtualizingCollectionBuilderOptional<TItem>
        {
            private ISyncPageStore<TItem> _pageStore;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new SyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher);
            }

            public IDataVirtualizingCollectionBuilderOptional<TItem> WithPageStore(
                ISyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                return this;
            }
        }

        private readonly int _count;

        private readonly ISyncPageStore<T> _pageStore;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private SyncDataVirtualizingCollection(ISyncPageStore<T> pageStore, ICountFetcher countFetcher)
        {
            _pageStore = pageStore;
            
            _compositeDisposable.Add(_pageStore);

            _count = countFetcher.CountFetch();
        }

        private T GetItemInner(int index)
        {
            return _pageStore.Fetch(index);
        }

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

        int GetCountInner() => _count;

        int ICollection<T>.Count => GetCountInner();

        int ICollection.Count => GetCountInner();

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsFixedSize => true;

        bool IList.IsReadOnly => true;

        bool ICollection<T>.IsReadOnly => true;

        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new object();

        public int IndexOf(object value)
        {
            return -1;
        }

        public int IndexOf(T item)
        {
            return -1;
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

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
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
            _compositeDisposable?.Dispose();
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