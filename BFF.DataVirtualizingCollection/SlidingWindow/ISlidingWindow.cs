namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Defines a nongeneric window to the backend (accessed by the page- and count-fetchers).
    /// A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
    /// Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
    /// makes it possible to go through the whole backend.
    /// </summary>
    public interface ISlidingWindow :
        IVirtualizationBase
    {
        /// <summary>
        /// Current offset of the window inside of the range of the items from the backend. The Offset marks the first item of the backend which is represented in the sliding window.
        /// </summary>
        int Offset { get; }
        
        /// <summary>
        /// Current maximum possible offset. Depends on the count of all backend items and the size of the window.
        /// </summary>
        int MaximumOffset { get; }

        /// <summary>
        /// Slides the window (<see cref="Offset"/>) to the backend one step to the start (left).
        /// </summary>
        void SlideLeft();
        
        /// <summary>
        /// Slides the window (<see cref="Offset"/>) to the backend one step to the end (right).
        /// </summary>
        void SlideRight();
        
        /// <summary>
        /// Sets the first entry of the window (<see cref="Offset"/>) to the given index of the backend.
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

        /// <summary>
        /// Sets windows size to given size.
        /// </summary>
        void SetWindowSizeTo(int size);
    }
    
    
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Defines a generic window to the backend (accessed by the page- and count-fetchers).
    /// A window is intended to be a much smaller section of the backend. It is specified by an offset and a size.
    /// Outwards it looks like a small list which contains only a few items of the whole backend. However, the sliding functionality
    /// makes it possible to go through the whole backend.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    public interface ISlidingWindow<T> :
        IVirtualizationBase<T>,
        ISlidingWindow
    {
    }
}
