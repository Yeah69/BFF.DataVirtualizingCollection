using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataAccesses
{
    /// <summary>
    /// The fundamental interface of the data virtualizing collection to get access to the data in task-based way.
    /// It should be possible to fetch the count and pages of data of arbitrary size.
    /// Additionally, a factory method in order to generate placeholders is required as well.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface IBasicTaskBasedAsyncDataAccess<T> : IBasicTaskBasedSyncDataAccess<T>, IPlaceholderFactory<T>
    {
    }

    /// <summary>
    /// The fundamental interface of the data virtualizing collection to get access to the data in task-based way.
    /// It should be possible to fetch the count and pages of data of arbitrary size.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface IBasicTaskBasedSyncDataAccess<T> : ITaskBasedPageFetcher<T>, ITaskBasedCountFetcher
    {
    }

    /// <summary>
    /// Provides a function to get a task which fetches a page from a data access.
    /// </summary>
    /// <typeparam name="T">The type of the collection items.</typeparam>
    public interface ITaskBasedPageFetcher<T>
    {
        /// <summary>
        /// Gets a task which fetches a page of given page-size starting at the starting at the given offset.
        /// </summary>
        /// <param name="offSet">Where the fetched page should start.</param>
        /// <param name="pageSize">Number of item of the fetched page.</param>
        /// <returns>See summary.</returns>
        Task<T[]> PageFetchAsync(int offSet, int pageSize);
    }

    /// <summary>
    /// Provides a function to fetch the count of elements stored in the data access.
    /// </summary>
    public interface ITaskBasedCountFetcher
    {
        /// <summary>
        /// Gets a task which fetches the current count of all items, which can be accessed through this collection.
        /// </summary>
        /// <returns>See summary.</returns>
        Task<int> CountFetchAsync();
    }
}