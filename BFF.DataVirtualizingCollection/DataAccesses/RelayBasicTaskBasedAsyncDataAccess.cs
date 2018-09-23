using System;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataAccesses
{
    /// <summary>
    /// An implementation of the <see cref="IBasicTaskBasedAsyncDataAccess{T}"/>, which gets the access functions injected.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    internal class RelayBasicTaskBasedAsyncDataAccess<T> : RelayBasicTaskBasedSyncDataAccess<T>, IBasicTaskBasedAsyncDataAccess<T>
    {
        private readonly Func<T> _placeHolderFactory;

        /// <summary>
        /// Initializes the <see cref="RelayBasicTaskBasedAsyncDataAccess{T}"/> object by injecting the access functions.
        /// </summary>
        /// <param name="pageFetcher">A function to get a task which fetches a page.</param>
        /// <param name="countFetcher">A function to get a task which fetches the item count.</param>
        /// <param name="placeHolderFactory">A function to create a placeholder.</param>
        internal RelayBasicTaskBasedAsyncDataAccess(Func<int, int, Task<T[]>> pageFetcher, Func<Task<int>> countFetcher, Func<T> placeHolderFactory)
            : base(pageFetcher, countFetcher)
        {
            _placeHolderFactory = placeHolderFactory;
        }

        /// <inheritdoc />
        public T CreatePlaceholder()
        {
            return _placeHolderFactory();
        }
    }

    /// <summary>
    /// An implementation of the <see cref="IBasicTaskBasedSyncDataAccess{T}"/>, which gets the access functions injected.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    internal class RelayBasicTaskBasedSyncDataAccess<T> : IBasicTaskBasedSyncDataAccess<T>
    {
        private readonly Func<int, int, Task<T[]>> _pageFetcher;
        private readonly Func<Task<int>> _countFetcher;

        /// <summary>
        /// Initializes the <see cref="RelayBasicTaskBasedSyncDataAccess{T}"/> object by injecting the access functions.
        /// </summary>
        /// <param name="pageFetcher">A function to get a task which fetches a page.</param>
        /// <param name="countFetcher">A function to get a task which fetches the item count.</param>
        internal RelayBasicTaskBasedSyncDataAccess(Func<int, int, Task<T[]>> pageFetcher, Func<Task<int>> countFetcher)
        {
            _pageFetcher = pageFetcher;
            _countFetcher = countFetcher;
        }

        /// <inheritdoc />
        public Task<T[]> PageFetchAsync(int offSet, int pageSize)
        {
            return _pageFetcher(offSet, pageSize);
        }

        /// <inheritdoc />
        public Task<int> CountFetchAsync()
        {
            return _countFetcher();
        }
    }
}