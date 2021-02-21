using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.PageStorage
{
    internal class LifoAsyncPageFetchScheduler : IAsyncPageFetchScheduler
    {
        private readonly Stack<(int Offset, TaskCompletionSource<Unit> TCS)> _stack = new();
        private readonly Subject<(int Offset, TaskCompletionSource<Unit> TCS)> _subject = new();

        public LifoAsyncPageFetchScheduler()
        {
            _subject
                .Do(tcs => _stack.Push(tcs))
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .Subscribe(_ =>
                {
                    Console.WriteLine("Throttle Emit");
                    while (_stack.Any())
                    {
                        var (offset, tcs) = _stack.Pop();
                        Console.WriteLine($"Pop: {offset}");
                        tcs.SetResult(Unit.Default);
                    }
                });
        }
        
        public Task Schedule(int offset, CancellationToken ct)
        {
            var taskCompletionSource = new TaskCompletionSource<Unit>();
            _subject.OnNext((offset, taskCompletionSource));
            return taskCompletionSource.Task;
        }
    }
}