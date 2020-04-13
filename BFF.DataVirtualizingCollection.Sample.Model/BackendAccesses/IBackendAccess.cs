namespace BFF.DataVirtualizingCollection.Sample.Model.BackendAccesses
{
    public interface IBackendAccess<T>
    {
        string Name { get; }
        
        T[] PageFetch(int pageOffset, int pageSize);

        T PlaceholderFetch(int pageOffset, int indexInsidePage);

        int CountFetch();
    }
}