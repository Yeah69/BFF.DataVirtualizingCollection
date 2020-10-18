using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.PageStorage
{
    public abstract class SyncNonPreloadingPageBaseTestsBase : PageTestsBase
    {
        internal abstract SyncNonPreloadingNonTaskBasedPage<int> PageWithFirstEntry69 { get; }

        internal abstract SyncNonPreloadingNonTaskBasedPage<IDisposable> PageWithDisposable(IDisposable disposable);

        [Fact]
        internal async Task Dispose_PageHasOneDisposable_DisposesImmediately()
        {
            // Arrange
            var isDisposed = false;
            var disposable = Disposable.Create(() => isDisposed = true);
            var sut = PageWithDisposable(disposable);

            // Act
            await sut.DisposeAsync();

            // Assert
            Assert.True(isDisposed);
        }

        [Fact]
        internal async Task Index_FetchFirstIndex_ReturnsValue()
        {
            // Arrange
            await using var sut = PageWithFirstEntry69;

            // Act
            var value = sut[0];

            // Assert
            Assert.Equal(69, value);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class SyncNonPreloadingNonTaskBasedPageTests : SyncNonPreloadingPageBaseTestsBase
    {
        internal override IPage<int> PageWithPageSizeOne =>
            new SyncNonPreloadingNonTaskBasedPage<int>(
                0, 
                1, 
                Disposable.Empty, 
                (offset, pageSize) => new[] { 69 });

        internal override SyncNonPreloadingNonTaskBasedPage<int> PageWithFirstEntry69 =>
            new SyncNonPreloadingNonTaskBasedPage<int>(
                0, 
                1, 
                Disposable.Empty, 
                (offset, pageSize) => new[] { 69 });

        internal override SyncNonPreloadingNonTaskBasedPage<IDisposable> PageWithDisposable(IDisposable disposable)
        {
            return new SyncNonPreloadingNonTaskBasedPage<IDisposable>(
                0, 
                1,
                Disposable.Empty, 
                (offset, pageSize) => new[] {disposable});
        }
    }
}
