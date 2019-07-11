using System;

namespace BFF.DataVirtualizingCollection.PageRemoval
{
    /// <summary>
    /// Thrown whenever an exception occurs during initialization or the process of page replacement
    /// </summary>
    public class PageReplacementStrategyException : Exception
    {
        internal PageReplacementStrategyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
