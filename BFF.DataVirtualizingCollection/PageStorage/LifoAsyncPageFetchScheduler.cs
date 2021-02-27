using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class LifoAsyncPageFetchScheduler : IAsyncPageFetchScheduler
    {
        private readonly Stack<TaskCompletionSource<Unit>> _stack = new();
        private readonly Subject<TaskCompletionSource<Unit>> _subject = new();

        public LifoAsyncPageFetchScheduler(
            TimeSpan throttleDueTime,
            IScheduler pageRequestBackgroundScheduler) =>
            _subject
                .Synchronize()
                .Do(tcs => _stack.Push(tcs))
                .Throttle(throttleDueTime, pageRequestBackgroundScheduler)
                .Subscribe(_ =>
                {
                    while (_stack.Any())
                    {
                        var tcs = _stack.Pop();
                        tcs.SetResult(Unit.Default);
                    }
                });

        public Task Schedule()
        {
            var taskCompletionSource = new TaskCompletionSource<Unit>();
            _subject.OnNext(taskCompletionSource);
            return taskCompletionSource.Task;
        }
    }
}