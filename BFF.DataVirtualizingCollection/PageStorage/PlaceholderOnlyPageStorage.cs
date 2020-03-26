using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class PlaceholderOnlyPageStorage<T> : IPageStorage<T>
    {
        private readonly int _pageSize;
        private readonly int _count;
        private readonly Func<int, int, T> _placeholderFactory;
        private readonly IScheduler _disposalScheduler;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private readonly ConcurrentDictionary<int, IPage<T>> _pages = new ConcurrentDictionary<int, IPage<T>>();

        internal PlaceholderOnlyPageStorage(
            int pageSize,
            int count,
            Func<int, int, T> placeholderFactory,
            IScheduler disposalScheduler)
        {
            _pageSize = pageSize;
            _count = count;
            _placeholderFactory = placeholderFactory;
            _disposalScheduler = disposalScheduler;
        }

        public T this[int index]
        {
            get
            {
                var pageKey = index / _pageSize;
                var pageIndex = index % _pageSize;

                var ret =_pages
                    .GetOrAdd(
                        pageKey,
                        FetchNonPreloadingPage)
                    [pageIndex];

                return ret;

                IPage<T> FetchNonPreloadingPage(int key)
                {
                    var offset = key * _pageSize;
                    var actualPageSize = Math.Min(_pageSize, _count - offset);
                    return new PlaceholderOnlyPage<T>(offset, actualPageSize, _placeholderFactory, _disposalScheduler);
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
            _compositeDisposable.Dispose();
        }
    }
}
