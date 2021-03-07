using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.PageStorage;
using NSubstitute;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.DataVirtualizingCollection
{
    public class AsyncDataVirtualizingCollectionTests
    {
        [Fact]
        public async Task Reset_BeforeInitialCountFetchCompleted_CountFetchIsCanceled()
        {
            // Arrange
            var wasCanceled = false;
            var sut = new AsyncDataVirtualizingCollection<int>(
                _ => Substitute.For<IPageStorage<int>>(),
                async ct =>
                {
                    try
                    {
                        await Task.Delay(10, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        wasCanceled = true;
                    }
                    return 10;
                },
                Substitute.For<IObservable<(int Offset, int PageSize, int[] PreviousPage, int[] Page)>>(),
                Substitute.For<IDisposable>(),
                new EventLoopScheduler(),
                TaskPoolScheduler.Default);

            // Act
            sut.Reset();
            await Task.Delay(50).ConfigureAwait(false);

            // Assert
            Assert.True(wasCanceled);
        }
    }
}