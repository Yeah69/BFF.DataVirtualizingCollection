using System.Linq;
using BFF.DataVirtualizingCollection.Sample.Model.PersistenceLink;

namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IMillionNumbersBackendAccess : IBackendAccess<long>
    {
    }

    internal class MillionNumbersBackendAccess : IMillionNumbersBackendAccess
    {
        private readonly IFetchMillionNumbersFromBackend _fetchMillionNumbersFromBackend;

        public MillionNumbersBackendAccess(
            IFetchMillionNumbersFromBackend fetchMillionNumbersFromBackend)
        {
            _fetchMillionNumbersFromBackend = fetchMillionNumbersFromBackend;
        }
        
        public string Name => "A Million Numbers Accessed Through Sqlite";

        public long[] PageFetch(int pageOffset, int pageSize)
        {
            return _fetchMillionNumbersFromBackend.FetchPage(pageOffset, pageSize);
        }

        public long PlaceholderFetch(int _, int __)
        {
            return -11L;
        }

        public long PreloadingPlaceholderFetch(int _, int __)
        {
            return -21L;
        }

        public int CountFetch()
        {
            return _fetchMillionNumbersFromBackend.CountFetch();
        }
    }
}