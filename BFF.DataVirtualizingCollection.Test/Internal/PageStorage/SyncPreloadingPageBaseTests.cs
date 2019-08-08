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
    public abstract class SyncPreloadingPageBaseTestsBase : PageTestsBase
    {
        internal abstract SyncPreloadingPageBase<int> PageConstructedWithNullPageFetcher { get; }
        internal abstract SyncPreloadingPageBase<int> PageConstructedWithNullScheduler { get; }
        internal abstract SyncPreloadingPageBase<int> PageWithFirstEntry69 { get; }

        internal abstract SyncPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable);

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
        internal void Index_FetchFirstIndex_ReturnsValue()
        {
            // Arrange
            using var sut = PageWithFirstEntry69;

            // Act
            var value = sut[0];

            // Assert
            Assert.Equal(69, value);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class SyncPreloadingNonTaskBasedPageTests : SyncPreloadingPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new SyncNonPreloadingNonTaskBasedPage<int>(0, 1, (offset, pageSize) => new[] { 69 });
        internal override SyncPreloadingPageBase<int> PageConstructedWithNullPageFetcher =>
            new SyncPreloadingNonTaskBasedPage<int>(0, 1, null, DefaultScheduler.Instance);

        internal override SyncPreloadingPageBase<int> PageConstructedWithNullScheduler =>
            new SyncPreloadingNonTaskBasedPage<int>(0, 1, (offset, pageSize) => new[] { 69 }, null);

        internal override SyncPreloadingPageBase<int> PageWithFirstEntry69 =>
            new SyncPreloadingNonTaskBasedPage<int>(0, 1, (offset, pageSize) => new[] { 69 }, DefaultScheduler.Instance);

        internal override SyncPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return new SyncPreloadingNonTaskBasedPage<IDisposable>(
                0,
                1,
                (offset, pageSize) =>
                {
                    Thread.Sleep(10);
                    return new[] { disposable };
                },
                DefaultScheduler.Instance);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class SyncPreloadingTaskBasedPageTests : SyncPreloadingPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new SyncPreloadingTaskBasedPage<int>(0, 1, (offset, pageSize) => Task.FromResult(new[] { 69 }), DefaultScheduler.Instance);

        internal override SyncPreloadingPageBase<int> PageConstructedWithNullScheduler =>
            new SyncPreloadingTaskBasedPage<int>(0, 1, (offset, pageSize) => Task.FromResult(new[] { 69 }), null);

        internal override SyncPreloadingPageBase<int> PageConstructedWithNullPageFetcher =>
            new SyncPreloadingTaskBasedPage<int>(0, 1, null, DefaultScheduler.Instance);

        internal override SyncPreloadingPageBase<int> PageWithFirstEntry69 =>
            new SyncPreloadingTaskBasedPage<int>(0, 1, (offset, pageSize) => Task.FromResult(new[] { 69 }), DefaultScheduler.Instance);

        internal override SyncPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return new SyncPreloadingTaskBasedPage<IDisposable>(
                0,
                1,
                async (offset, pageSize) =>
                {
                    await Task.Delay(10);
                    return new[] { disposable };
                },
                DefaultScheduler.Instance);
        }
    }
}
