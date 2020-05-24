using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageRemoval;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IPageStorage<out T> : IDisposable
    {
        T this[int index] { get; }
    }

    internal class PageStorage<T> : IPageStorage<T>
    {
        private readonly int _pageSize;
        private readonly int _count;
        private readonly Func<int, int, int, IPage<T>> _nonPreloadingPageFactory;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        protected readonly int PageCount;
        protected bool IsDisposed;
        protected readonly object IsDisposedLock = new object();
        protected readonly ConcurrentDictionary<int, IPage<T>> Pages = new ConcurrentDictionary<int, IPage<T>>();
        protected readonly ISubject<(int PageKey, int PageIndex)> Requests;

        internal PageStorage(
            int pageSize,
            int count,
            Func<int, int, int, IPage<T>> nonPreloadingPageFactory,
            Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
        {
            _pageSize = pageSize;
            _count = count;
            PageCount = count % pageSize == 0 
                ? count / pageSize 
                : count / pageSize + 1;
            _nonPreloadingPageFactory = nonPreloadingPageFactory;
            Requests = new Subject<(int PageKey, int PageIndex)>().AddTo(_compositeDisposable);
            pageReplacementStrategyFactory(Requests)
                .Subscribe(pageKeysToRemove =>
                    {
                        foreach (var pageKey in pageKeysToRemove)
                        {
                            if (Pages.TryRemove(pageKey, out var page))
                            {
                                page.Dispose();
                            }
                        }
                    },
                    exception => throw new PageReplacementStrategyException(
                        "LeastRecentlyUsed strategy: Something unexpected happened during page removal! See inner exception.",
                        exception))
                .AddTo(_compositeDisposable);
        }

        public T this[int index]
        {
            get
            {
                var pageKey = index / _pageSize;
                var pageIndex = index % _pageSize;

                lock (IsDisposedLock)
                {
                    if (!IsDisposed) Requests.OnNext((pageKey, pageIndex));
                }

                var ret = Pages
                    .GetOrAdd(
                        pageKey,
                        FetchNonPreloadingPage)
                    [pageIndex];

                Preloading(pageKey);

                return ret;

                IPage<T> FetchNonPreloadingPage(int key) => FetchPage(key, _nonPreloadingPageFactory);
            }
        }

        protected virtual void Preloading(int pageKey)
        { }

        protected IPage<T> FetchPage(int key, Func<int, int, int, IPage<T>> fetcher)
        {
            var offset = key * _pageSize;
            var actualPageSize = Math.Min(_pageSize, _count - offset);
            return fetcher(key, offset, actualPageSize);
        }

        public void Dispose()
        {
            var pages = Pages.Values.ToList();
            Pages.Clear();
            foreach (var page in pages)
            {
                page.Dispose();
            }

            lock (IsDisposedLock)
            {
                _compositeDisposable.Dispose();
                IsDisposed = true;
            }
        }
    }
}
