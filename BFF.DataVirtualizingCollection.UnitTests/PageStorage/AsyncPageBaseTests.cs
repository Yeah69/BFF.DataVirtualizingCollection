using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.PageStorage
{
    public abstract class AsyncPageBaseTestsBase : PageTestsBase
    {
        internal abstract AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 { get; }

        internal abstract AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable);

        internal abstract AsyncPageBase<IDisposable> PageWithDisposablePlaceholder(IDisposable disposable);

        [Fact]
        internal async Task Dispose_PageHasOneDisposable_Disposes()
        {
            // Arrange
            var isDisposed = new TaskCompletionSource<Unit>();
            var disposable = Disposable.Create(() => isDisposed.SetResult(Unit.Default));
            var sut = PageWithDisposable(disposable);

            // Act
            await sut.DisposeAsync().ConfigureAwait(false);

            // Assert
            await isDisposed.Task.ToObservable().Timeout(TimeSpan.FromMinutes(5)).ToTask();
        }

        [Fact]
        internal async Task Index_FetchFirstIndex_ReturnsPlaceholderImmediately()
        {
            // Arrange
            await using var sut = PageWithFirstEntry69AndPlaceholder23;

            // Act
            var value = sut[0];

            // Assert
            Assert.Equal(23, value);
        }

        [Fact]
        internal async Task Index_FetchFirstIndex_ReturnsValueAfterWaiting()
        {
            // Arrange
            await using var sut = PageWithFirstEntry69AndPlaceholder23;

            // Act
            await Task.Delay(250).ConfigureAwait(false);
            var value = sut[0];

            // Assert
            Assert.Equal(69, value);
        }

        [Fact]
        internal async Task Index_PlaceholderIsDisposable_DisposePlaceholderWhenActualPageArrives()
        {
            // Arrange
            var isDisposed = new TaskCompletionSource<Unit>();
            var disposable = Disposable.Create(() => isDisposed.SetResult(Unit.Default));
            await using var sut = PageWithDisposablePlaceholder(disposable);

            // Act
            var _ = sut[0];

            // Assert
            await isDisposed.Task.ToObservable().Timeout(TimeSpan.FromSeconds(5)).ToTask();
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class AsyncNonTaskBasedPageTests : AsyncPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new AsyncNonTaskBasedPage<int>(
                0,
                0, 
                1,
                Disposable.Empty, 
                (offset, pageSize, _) =>
                {
                    Thread.Sleep(10);
                    return new[] { 69 };
                }, 
                (_, __) => 23, 
                DefaultScheduler.Instance, 
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 =>
            new AsyncNonTaskBasedPage<int>(
                0,
                0,
                1,
                Disposable.Empty, 
                (offset, pageSize, _) =>
                {
                    Thread.Sleep(10);
                    return new[] {69};
                },
                (_, __) => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return
                new AsyncNonTaskBasedPage<IDisposable>(
                    0,
                    0,
                    1,
                    Disposable.Empty, 
                    (offset, pageSize, _) =>
                    {
                        Thread.Sleep(10);
                        return new[] { disposable };
                    },
                    (_, __) => Disposable.Empty,
                    DefaultScheduler.Instance,
                    Observer.Create<(int Offset, int PageSize, IDisposable[] PreviousPage, IDisposable[] Page)>(_ => { }));
        }

        internal override AsyncPageBase<IDisposable> PageWithDisposablePlaceholder(IDisposable disposable)
        {
            return
                new AsyncNonTaskBasedPage<IDisposable>(
                    0,
                    0,
                    1,
                    Disposable.Empty, 
                    (offset, pageSize, _) =>
                    {
                        Thread.Sleep(10);
                        return new []{ Disposable.Empty };
                    },
                    (_, __) => disposable,
                    DefaultScheduler.Instance,
                    Observer.Create<(int Offset, int PageSize, IDisposable[] PreviousPage, IDisposable[] Page)>(_ => { }));
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class AsyncTaskBasedPageTests : AsyncPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new AsyncTaskBasedPage<int>(
                0,
                0,
                1,
                Disposable.Empty, 
                async (offset, pageSize, _) =>
                {
                    await Task.Delay(10);
                    return new[] { 69 };
                },
                (_, __) => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 =>
            new AsyncTaskBasedPage<int>(
                0,
                0,
                1,
                Disposable.Empty, 
                async (offset, pageSize, _) =>
                {
                    await Task.Delay(10);
                    return new[] {69};
                },
                (_, __) => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return
                new AsyncTaskBasedPage<IDisposable>(
                    0,
                    0,
                    1,
                    Disposable.Empty, 
                    async (offset, pageSize, _) =>
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                        return new[] { disposable };
                    },
                    (_, __) => Disposable.Empty,
                    DefaultScheduler.Instance,
                    Observer.Create<(int Offset, int PageSize, IDisposable[] PreviousPage, IDisposable[] Page)>(_ => { }));
        }

        internal override AsyncPageBase<IDisposable> PageWithDisposablePlaceholder(IDisposable disposable)
        {
            return
                new AsyncTaskBasedPage<IDisposable>(
                    0,
                    0,
                    1,
                    Disposable.Empty, 
                    async (offset, pageSize, _) =>
                    {
                        await Task.Delay(10);
                        return new[] { Disposable.Empty };
                    },
                    (_, __) => disposable,
                    DefaultScheduler.Instance,
                    Observer.Create<(int Offset, int PageSize, IDisposable[] PreviousPage, IDisposable[] Page)>(_ => { }));
        }
    }
}
