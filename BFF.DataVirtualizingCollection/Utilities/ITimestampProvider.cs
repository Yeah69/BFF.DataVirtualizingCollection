using System;

namespace BFF.DataVirtualizingCollection.Utilities
{
    internal interface ITimestampProvider
    {
        DateTime Now { get; }
    }
}
