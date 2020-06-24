namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Marks a nongeneric data virtualizing collection.
    /// The data virtualizing collection represents the whole backend as a list. However, the items are not loaded all at once but page by page on demand.
    /// </summary>
    public interface IDataVirtualizingCollection : IVirtualizationBase
    {
    }
    
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Marks a generic data virtualizing collection.
    /// The data virtualizing collection represents the whole backend as a list. However, the items are not loaded all at once but page by page on demand.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    public interface IDataVirtualizingCollection<T> :
        IVirtualizationBase<T>,
        IDataVirtualizingCollection
    {
    }
}