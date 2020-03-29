namespace BFF.DataVirtualizingCollection.Test.Integration
{
    public enum PageLoadingBehavior
    {
        Preloading,
        NonPreloading
    }

    public enum PageRemovalBehavior
    {
        Hoarding,
        LeastRecentlyUsed
    }

    public enum FetchersKind
    {
        NonTaskBased,
        TaskBased
    }

    public enum IndexAccessBehavior
    {
        Asynchronous,
        Synchronous
    }
}