using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageRemoval;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal interface IPageStorage<out T> : IAsyncDisposable
    {
        T this[int index] { get; }

        Task Reset(int newCount);
    }

    internal class PageStorage<T> : IPageStorage<T>
    {
        private readonly int _pageSize;
        private readonly Func<int, int, int, IDisposable, IPage<T>> _nonPreloadingPageFactory;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        protected readonly int PageCount;
        protected bool IsDisposed;
        protected readonly object IsDisposedLock = new object();
        protected readonly ConcurrentDictionary<int, IPage<T>> Pages = new ConcurrentDictionary<int, IPage<T>>();
        protected readonly ISubject<(int PageKey, int PageIndex)> Requests;

        private int _count;
        
        internal PageStorage(
            int pageSize,
            int count,
            Func<int, int, int, IDisposable, IPage<T>> nonPreloadingPageFactory,
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
                .SelectMany(async pageKeysToRemove =>
                {
                    var disposables = new List<Task>();
                    foreach (var pageKey in pageKeysToRemove)
                    {
                        if (Pages.TryGetValue(pageKey, out var page))
                            disposables.Add(page.DisposeAsync().AsTask());
                    }

                    await Task.WhenAll(disposables);
                    return Unit.Default;
                })
                .Subscribe(_ => { },
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

        public Task Reset(int newCount)
        {
            _count = newCount;

            return Task.WhenAll(
                Pages.Values.ToList().Select(p => p.DisposeAsync().AsTask()));

        }

        protected virtual void Preloading(int pageKey)
        { }

        protected IPage<T> FetchPage(int key, Func<int, int, int, IDisposable, IPage<T>> fetcher)
        {
            var offset = key * _pageSize;
            var actualPageSize = Math.Min(_pageSize, _count - offset);
            var disposable = Disposable.Create(() => Pages.TryRemove(key, out _));
            return fetcher(key, offset, actualPageSize, disposable);
        }

        public async ValueTask DisposeAsync()
        {
            var pages = Pages.Values.ToList();
            Pages.Clear();
            
            await Task.WhenAll(
                pages.Select(p => p.DisposeAsync().AsTask()));

            lock (IsDisposedLock)
            {
                _compositeDisposable.Dispose();
                IsDisposed = true;
            }
        }
    }
}
