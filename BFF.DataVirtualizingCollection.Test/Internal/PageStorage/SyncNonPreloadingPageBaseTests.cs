using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.Internal.PageStorage
{
    public abstract class SyncNonPreloadingPageBaseTestsBase : PageTestsBase
    {
        internal abstract SyncNonPreloadingPageBase<int> PageConstructedWithNullPageFetcher { get; }
        internal abstract SyncNonPreloadingPageBase<int> PageWithFirstEntry69 { get; }

        internal abstract SyncNonPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable);

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
        internal void Dispose_PageHasOneDisposable_DisposesImmediately()
        {
            // Arrange
            var isDisposed = false;
            var disposable = Disposable.Create(() => isDisposed = true);
            var sut = PageWithDisposable(disposable);

            // Act
            sut.Dispose();

            // Assert
            Assert.True(isDisposed);
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

    public class SyncNonPreloadingNonTaskBasedPageTests : SyncNonPreloadingPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new SyncNonPreloadingNonTaskBasedPage<int>(0, 1, (offset, pageSize) => new[] { 69 });
        internal override SyncNonPreloadingPageBase<int> PageConstructedWithNullPageFetcher =>
            new SyncNonPreloadingNonTaskBasedPage<int>(0, 1, null);

        internal override SyncNonPreloadingPageBase<int> PageWithFirstEntry69 =>
            new SyncNonPreloadingNonTaskBasedPage<int>(0, 1, (offset, pageSize) => new[] { 69 });

        internal override SyncNonPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return new SyncNonPreloadingNonTaskBasedPage<IDisposable>(0, 1, (offset, pageSize) => new[] {disposable});
        }
    }

    public class SyncNonPreloadingTaskBasedPageTests : SyncNonPreloadingPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new SyncNonPreloadingTaskBasedPage<int>(0, 1, (offset, pageSize) => Task.FromResult(new[] { 69 }));
        internal override SyncNonPreloadingPageBase<int> PageConstructedWithNullPageFetcher =>
            new SyncNonPreloadingTaskBasedPage<int>(0, 1, null);

        internal override SyncNonPreloadingPageBase<int> PageWithFirstEntry69 =>
            new SyncNonPreloadingTaskBasedPage<int>(0, 1, (offset, pageSize) => Task.FromResult(new[] { 69 }));

        internal override SyncNonPreloadingPageBase<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return new SyncNonPreloadingTaskBasedPage<IDisposable>(0, 1, (offset, pageSize) => Task.FromResult(new[] { disposable }));
        }
    }
}
