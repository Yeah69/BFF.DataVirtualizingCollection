using System;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.Extensions;
using BFF.DataVirtualizingCollection.PageStorage;
using JetBrains.Annotations;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal sealed class SyncDataVirtualizingCollection<T> : DataVirtualizingCollectionBase<T>
    {
        private readonly IPageStorage<T> _pageStorage;

        internal SyncDataVirtualizingCollection(
            [NotNull] Func<int, IPageStorage<T>> pageStoreFactory,
            [NotNull] Func<int> countFetcher)
        {
            pageStoreFactory = pageStoreFactory ?? throw new ArgumentNullException(nameof(pageStoreFactory));
            countFetcher = countFetcher ?? throw new ArgumentNullException(nameof(countFetcher));

            Count = countFetcher();
            _pageStorage = pageStoreFactory(Count).AddTo(CompositeDisposable);
        }

        protected override int Count { get; }

        protected override T GetItemInner(int index)
        {
            return _pageStorage[index];
        }

        public override Task InitializationCompleted { get; } = Task.CompletedTask;
    }
}