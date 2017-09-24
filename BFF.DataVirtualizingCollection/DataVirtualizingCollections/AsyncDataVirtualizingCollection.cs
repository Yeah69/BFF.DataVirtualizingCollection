using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.PageStores;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal class AsyncDataVirtualizingCollection<T> : IDataVirtualizingCollection<T>
    {
        internal static IBuilderRequired<T> CreateBuilder() => new Builder<T>();

        internal interface IBuilderRequired<TItem>
        {
            IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher,
                IScheduler subscribeScheduler,
                IScheduler observeScheduler);
        }
        internal interface IBuilderOptional<TItem>
        {
            IDataVirtualizingCollection<TItem> Build();
        }

        internal class Builder<TItem> : IBuilderRequired<TItem>, IBuilderOptional<TItem>
        {
            private IAsyncPageStore<TItem> _pageStore;
            private IScheduler _subscribeScheduler;
            private IScheduler _observeScheduler;
            private ICountFetcher _countFetcher;

            

            public IDataVirtualizingCollection<TItem> Build()
            {
                return new AsyncDataVirtualizingCollection<TItem>(
                    _pageStore, 
                    _countFetcher, 
                    _subscribeScheduler,
                    _observeScheduler);
            }

            public IBuilderOptional<TItem> WithPageStore(
                IAsyncPageStore<TItem> pageStore,
                ICountFetcher countFetcher,
                IScheduler subscribeScheduler,
                IScheduler observeScheduler)
            {
                _pageStore = pageStore;
                _countFetcher = countFetcher;
                _observeScheduler = observeScheduler;
                _subscribeScheduler = subscribeScheduler;
                return this;
            }
        }

        private readonly int _count;

        private readonly IAsyncPageStore<T> _pageStore;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private AsyncDataVirtualizingCollection(IAsyncPageStore<T> pageStore, ICountFetcher countFetcher, IScheduler subscribeScheduler, IScheduler observeScheduler)
        {
            _pageStore = pageStore;

            var disposable = _pageStore.OnCollectionChangedReplace
                .SubscribeOn(subscribeScheduler)
                .ObserveOn(observeScheduler)
                .Subscribe(tuple => OnCollectionChangedReplace(tuple.Item1, tuple.Item2, tuple.Item3));
            _compositeDisposable.Add(disposable);
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