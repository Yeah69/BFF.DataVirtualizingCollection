namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Defines a window to the backend (accessed by the page- and count-fetchers).
    /// A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
    /// Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
    /// makes it possible to go through the whole backend.
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface ISlidingWindow :
        IVirtualizationBase
    {
        /// <summary>
        /// Current offset of the window inside of the range of the items from the backend.
        /// </summary>
        int Offset { get; }
        
        /// <summary>
        /// Current maximum possible offset.
        /// </summary>
        int MaximumOffset { get; }

        /// <summary>
        /// Slides the window to the backend one step to the left (bottom).
        /// </summary>
        void SlideLeft();
        
        /// <summary>
        /// Slides the window to the backend one step to the right (top).
        /// </summary>
        void SlideRight();
        
        /// <summary>
        /// Sets the first entry of the window to the given index of the backend.
        /// </summary>
        void JumpTo(int index);
        
        /// <summary>
        /// Increases windows size by one.
        /// </summary>
        void IncreaseWindowSize();
        
        /// <summary>
        /// Decreases windows size by one.
        /// </summary>
        void DecreaseWindowSize();
        
        /// <summary>
        /// Increases windows size by given increment.
        /// </summary>
        void IncreaseWindowSizeBy(int sizeIncrement);
        
        /// <summary>
        /// Decreases windows size by given increment.
        /// </summary>
        void DecreaseWindowSizeBy(int sizeIncrement);
    }
    
    
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Defines a window to the backend (accessed by the page- and count-fetchers).
    /// A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
    /// Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
    /// makes it possible to go through the whole backend.
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface ISlidingWindow<T> :
        IVirtualizationBase<T>,
        ISlidingWindow
    {
    }
}
