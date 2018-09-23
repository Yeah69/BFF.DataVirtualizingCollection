using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace BFF.DataVirtualizingCollection.Extensions
{
    internal static class SchedulerExtensions
    {
        internal static void MinimalSchedule(this IScheduler scheduler, Action action) => scheduler.Schedule(
            Unit.Default,
            (_, __) =>
            {
                action();
                return Disposable.Empty;
            });
    }
}
