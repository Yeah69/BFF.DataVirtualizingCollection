namespace BFF.DataVirtualizingCollection.Sample.Model.PersistenceLink
{
    public interface IFetchMillionNumbersFromBackend
    {
        long[] FetchPage(int pageOffset, int pageSize);

        int CountFetch();
    }
}