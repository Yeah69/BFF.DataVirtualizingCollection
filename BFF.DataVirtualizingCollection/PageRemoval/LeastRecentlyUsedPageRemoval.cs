using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using BFF.DataVirtualizingCollection.Utilities;

namespace BFF.DataVirtualizingCollection.PageRemoval
{
    internal static class LeastRecentlyUsedPageRemoval
    {
        internal static Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>> Create(
            int pageLimit,
            int removalCount,
            bool isPreloading,
            ITimestampProvider timestampProvider)
        {
            IDictionary<int, DateTime> lastUsage = new Dictionary<int, DateTime>();
            return pageRequests =>
            {
                if (isPreloading)
                {
                    // Make sure at least three pages (because of the preloading the two pages need to be
                    // considered for the neighbor pages of the last fetched page) stay in the page store
                    pageLimit = pageLimit < 4 ? 4 : pageLimit;
                    removalCount = removalCount < 1
                        ? 1
                        : removalCount > pageLimit - 3
                            ? pageLimit - 3
                            : removalCount;
                }
                else
                {
                    // Make sure at least one page stays in the page store
                    pageLimit = pageLimit < 2 ? 2 : pageLimit;
                    removalCount = removalCount < 1
                        ? 1
                        : removalCount > pageLimit - 1
                            ? pageLimit - 1
                            : removalCount;
                }

                return pageRequests.Select(tuple =>
                    {
                        lastUsage[tuple.PageKey] = timestampProvider.Now;

                        // Don't request any page removals if page limit isn't reached
                        if (lastUsage.Count <= pageLimit) return new ReadOnlyCollection<int>(new List<int>());

                        // Remove at least one page but as much as required to get below page limit
                        // The "-1" is necessary because the currently requested page is included in the count of the last usages
                        var actualRemovalCount = lastUsage.Count - 1 - pageLimit + removalCount;
                        var removalRequests = new ReadOnlyCollection<int>(
                            lastUsage
                                // Sort by least recent usage
                                .OrderBy(kvp => kvp.Value)
                                .Take(actualRemovalCount)
                                .Select(kvp => kvp.Key)
                                .ToList());
                        // Assuming the page will be removed, they need to be removed from the last-usage tracking here as well
                        foreach (var removalRequest in removalRequests)
                        {
                            lastUsage.Remove(removalRequest);
                        }

                        return removalRequests;
                    })
                    .Where(list => list.Any());
            };
        }
    }
}
