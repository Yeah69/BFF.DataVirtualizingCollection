using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageRemoval;
using JetBrains.Annotations;

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
        private readonly int _pageCount;
        private readonly bool _isPreloadingActive;
        private readonly Func<int, int, IPage<T>> _nonPreloadingPage;
        private readonly Func<int, int, IPage<T>> _preloadingPage;
        private readonly ConcurrentDictionary<int, IPage<T>> _pages = new ConcurrentDictionary<int, IPage<T>>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ISubject<(int PageKey, int PageIndex)> _requests;

        internal PageStorage(
            int pageSize,
            int count,
            bool isPreloadingActive,
            [NotNull] Func<int, int, IPage<T>> nonPreloadingPage,
            [CanBeNull] Func<int, int, IPage<T>> preloadingPage,
            [NotNull] Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> pageReplacementStrategyFactory)
        {
            nonPreloadingPage = nonPreloadingPage ?? throw new ArgumentNullException(nameof(nonPreloadingPage));
            pageReplacementStrategyFactory = pageReplacementStrategyFactory ?? throw new ArgumentNullException(nameof(pageReplacementStrategyFactory));
            if (!isPreloadingActive && preloadingPage is null) throw new ArgumentNullException(nameof(preloadingPage));

            _pageSize = pageSize;
            _count = count;
            _pageCount = count % pageSize == 0 
                ? count / pageSize 
                : count / pageSize + 1;
            _isPreloadingActive = isPreloadingActive;
            _nonPreloadingPage = nonPreloadingPage;
            _preloadingPage = preloadingPage;
            _requests = new Subject<(int PageKey, int PageIndex)>().AddTo(_compositeDisposable);
            pageReplacementStrategyFactory(_requests)
                .Subscribe(pageKeysToRemove =>
                    {
                        foreach (var pageKey in pageKeysToRemove)
                        {
                            if (_pages.TryRemove(pageKey, out var page))
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

                _requests.OnNext((pageKey, pageIndex));

                var ret =_pages
                    .GetOrAdd(
                        pageKey,
                        FetchNonPreloadingPage)
                    [pageIndex];

                if (_isPreloadingActive)
                {
                    var previousPageKey = pageKey - 1;
                    if (previousPageKey >= 0)
                        _pages.GetOrAdd(
                            previousPageKey,
                            FetchPreloadingPage);
                    var nextPageKey = pageKey + 1;
                    if (nextPageKey < _pageCount)
                        _pages.GetOrAdd(
                            nextPageKey,
                            FetchPreloadingPage);
                }

                return ret;

                IPage<T> FetchNonPreloadingPage(int key) => FetchPage(key, _nonPreloadingPage);

                IPage<T> FetchPreloadingPage(int key)
                {
                    _requests.OnNext((key, -1));

                    return FetchPage(key, _preloadingPage);
                }

                IPage<T> FetchPage(int key, Func<int, int, IPage<T>> fetcher)
                {
                    var offset = key * _pageSize;
                    var actualPageSize = Math.Min(_pageSize, _count - offset);
                    return fetcher(offset, actualPageSize);
                }
            }
        }

        public void Dispose()
        {
            var pages = _pages.Values.ToList();
            _pages.Clear();
            foreach (var page in pages)
            {
                page.Dispose();
            }
            _compositeDisposable?.Dispose();
        }
    }
}
