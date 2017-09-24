using System;

namespace BFF.DataVirtualizingCollection.DataAccesses
{
    /// <summary>
    /// An implementation of the <see cref="IBasicAsyncDataAccess{T}"/>, which gets the access functions injected.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class RelayBasicAsyncDataAccess<T> : RelayBasicSyncDataAccess<T>, IBasicAsyncDataAccess<T>
    {
        private readonly Func<T> _placeHolderFactory;

        /// <summary>
        /// Initializes the <see cref="RelayBasicAsyncDataAccess{T}"/> object by injecting the access functions.
        /// </summary>
        /// <param name="pageFetcher">A function to fetch a page.</param>
        /// <param name="countFetcher">A function to fetch the item count.</param>
        /// <param name="placeHolderFactory">A function to create a placeholder.</param>
        public RelayBasicAsyncDataAccess(Func<int, int, T[]> pageFetcher, Func<int> countFetcher, Func<T> placeHolderFactory)
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
    /// An implementation of the <see cref="IBasicAsyncDataAccess{T}"/>, which gets the access functions injected.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public class RelayBasicSyncDataAccess<T> : IBasicSyncDataAccess<T>
    {
        private readonly Func<int, int, T[]> _pageFetcher;
        private readonly Func<int> _countFetcher;

        /// <summary>
        /// Initializes the <see cref="RelayBasicAsyncDataAccess{T}"/> object by injecting the access functions.
        /// </summary>
        /// <param name="pageFetcher">A function to fetch a page.</param>
        /// <param name="countFetcher">A function to fetch the item count.</param>
        public RelayBasicSyncDataAccess(Func<int, int, T[]> pageFetcher, Func<int> countFetcher)
        {
            _pageFetcher = pageFetcher;
            _countFetcher = countFetcher;
        }

        /// <inheritdoc />
        public T[] PageFetch(int offSet, int pageSize)
        {
            return _pageFetcher(offSet, pageSize);
        }

        /// <inheritdoc />
        public int CountFetch()
        {
            return _countFetcher();
        }
    }
}