using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace BFF.DataVirtualizingCollection.PageRemoval
{
    internal static class HoardingPageNonRemoval
    {
        internal static Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> Create() => 
            _ => Observable.Never<IReadOnlyList<int>>();
    }
}
