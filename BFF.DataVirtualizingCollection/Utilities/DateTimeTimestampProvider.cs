using System;

namespace BFF.DataVirtualizingCollection.Utilities
{
    internal class DateTimeTimestampProvider : ITimestampProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
