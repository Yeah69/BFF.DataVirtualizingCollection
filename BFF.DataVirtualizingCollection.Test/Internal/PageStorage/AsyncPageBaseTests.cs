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
// ReSharper disable AssignNullToNotNullAttribute *** Testing exception on null passing

namespace BFF.DataVirtualizingCollection.Test.Internal.PageStorage
{
    public abstract class AsyncPageBaseTestsBase : PageTestsBase
    {
        internal abstract AsyncPageBase<int> PageConstructedWithNullPageFetcher { get; }
        internal abstract AsyncPageBase<int> PageConstructedWithNullPlaceholderFactory { get; }
        internal abstract AsyncPageBase<int> PageConstructedWithNullScheduler { get; }
        internal abstract AsyncPageBase<int> PageConstructedWithNullObserverFactory { get; }
        internal abstract AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 { get; }

        internal abstract AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable);

        [Fact]
        internal void Constructor_PageFetcherNull_ThrowsArgumentNullException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                using var sut = PageConstructedWithNullPageFetcher;
                return sut;
            });
        }

        [Fact]
        internal void Constructor_PlaceholderFactoryNull_ThrowsArgumentNullException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                using var sut = PageConstructedWithNullPlaceholderFactory;
                return sut;
            });
        }

        [Fact]
        internal void Constructor_SchedulerNull_ThrowsArgumentNullException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                using var sut = PageConstructedWithNullScheduler;
                return sut;
            });
        }

        [Fact]
        internal void Constructor_ObserverNull_ThrowsArgumentNullException()
        {
            // Arrange + Act + Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                using var sut = PageConstructedWithNullObserverFactory;
                return sut;
            });
        }

        [Fact]
        internal async Task Dispose_PageHasOneDisposable_Disposes()
        {
            // Arrange
            var isDisposed = new TaskCompletionSource<Unit>();
            var disposable = Disposable.Create(() => isDisposed.SetResult(Unit.Default));
            var sut = PageWithDisposable(disposable);

            // Act
            sut.Dispose();

            // Assert
            await isDisposed.Task.ToObservable().Timeout(TimeSpan.FromMinutes(5)).ToTask();
        }

        [Fact]
        internal void Index_FetchFirstIndex_ReturnsPlaceholderImmediately()
        {
            // Arrange
            using var sut = PageWithFirstEntry69AndPlaceholder23;

            // Act
            var value = sut[0];

            // Assert
            Assert.Equal(23, value);
        }

        [Fact]
        internal async Task Index_FetchFirstIndex_ReturnsValueAfterWaiting()
        {
            // Arrange
            using var sut = PageWithFirstEntry69AndPlaceholder23;

            // Act
            await Task.Delay(250).ConfigureAwait(false);
            var value = sut[0];

            // Assert
            Assert.Equal(69, value);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class AsyncNonTaskBasedPageTests : AsyncPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new AsyncNonTaskBasedPage<int>(
                0, 
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] { 69 };
                }, 
                () => 23, 
                DefaultScheduler.Instance, 
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));
        internal override AsyncPageBase<int> PageConstructedWithNullPageFetcher =>
            new AsyncNonTaskBasedPage<int>(
                0,
                1,
                null,
                () => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullPlaceholderFactory =>
            new AsyncNonTaskBasedPage<int>(
                0,
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] { 69 };
                },
                null,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullScheduler =>
            new AsyncNonTaskBasedPage<int>(
                0,
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] { 69 };
                },
                () => 23,
                null,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullObserverFactory =>
            new AsyncNonTaskBasedPage<int>(
                0,
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] { 69 };
                },
                () => 23,
                DefaultScheduler.Instance,
                null);

        internal override AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 =>
            new AsyncNonTaskBasedPage<int>(
                0,
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] {69};
                },
                () => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return
                new AsyncNonTaskBasedPage<IDisposable>(
                    0,
                    1,
                    (offset, pageSize) =>
                    {
                        Thread.Sleep(10);
                        return new[] { disposable };
                    },
                    () => Disposable.Empty,
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
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] { 69 };
                },
                () => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));
        internal override AsyncPageBase<int> PageConstructedWithNullPageFetcher =>
            new AsyncTaskBasedPage<int>(
                0,
                1,
                null,
                () => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullPlaceholderFactory =>
            new AsyncTaskBasedPage<int>(
                0,
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] { 69 };
                },
                null,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullScheduler =>
            new AsyncTaskBasedPage<int>(
                0,
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] { 69 };
                },
                () => 23,
                null,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<int> PageConstructedWithNullObserverFactory =>
            new AsyncTaskBasedPage<int>(
                0,
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] { 69 };
                },
                () => 23,
                DefaultScheduler.Instance,
                null);

        internal override AsyncPageBase<int> PageWithFirstEntry69AndPlaceholder23 =>
            new AsyncTaskBasedPage<int>(
                0,
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] {69};
                },
                () => 23,
                DefaultScheduler.Instance,
                Observer.Create<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>(_ => { }));

        internal override AsyncPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return
                new AsyncTaskBasedPage<IDisposable>(
                    0,
                    1,
                    async (offset, pageSize) =>
                    {
                        await Task.Delay(10);
                        return new[] { disposable };
                    },
                    () => Disposable.Empty,
                    DefaultScheduler.Instance,
                    Observer.Create<(int Offset, int PageSize, IDisposable[] PreviousPage, IDisposable[] Page)>(_ => { }));
        }
    }
}
