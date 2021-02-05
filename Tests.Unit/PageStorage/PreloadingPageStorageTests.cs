using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using Xunit;

// ReSharper disable UnusedVariable *** Necessary for some tests
// ReSharper disable AccessToDisposedClosure *** Controlled disposal

namespace BFF.DataVirtualizingCollection.Test.PageStorage
{
    public class PreloadingPageStorageTests
    {
        [Fact]
        public async Task Index_RequestSeventhPage_SeventhSixthAndEighthPageRequested()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            using var requests = new ReplaySubject<(int PageKey, int PageIndex)>();
            await using var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___, ____) => page,
                (_, __, ___, ____) => page,
                req =>
                {
                    req.Subscribe(requests);
                    return Observable.Never<IReadOnlyList<int>>();
                });

            // Act
            var unused = sut[69];
            requests.OnCompleted();

            // Assert
            Assert.Collection(
                requests.ToEnumerable(),
                tuple =>
                {
                    var (pageKey, pageIndex) = tuple;
                    Assert.Equal(6, pageKey);
                    Assert.Equal(9, pageIndex);
                },
                tuple =>
                {
                    var (pageKey, pageIndex) = tuple;
                    Assert.Equal(7, pageKey);
                    Assert.Equal(-1, pageIndex);
                },
                tuple =>
                {
                    var (pageKey, pageIndex) = tuple;
                    Assert.Equal(5, pageKey);
                    Assert.Equal(-1, pageIndex);
                });
        }

        [Fact]
        public async Task Dispose_ThreePagesRequested_SevenPagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___, ____) => page,
                (_, __, ___, ____) => page,
                _ => Observable.Never<IReadOnlyList<int>>());

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            await sut.DisposeAsync().ConfigureAwait(false);

            // Assert
            await page.Received(Quantity.Exactly(7)).DisposeAsync();
        }

        [Fact]
        public async Task PageRemoval_RemoveAllExceptFirstRequestedPage_SixPagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___, ____) => page,
                (_, __, ___, ____) => page,
                _ => subject);

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            subject.OnNext(new[] { 0, 1, 2, 3, 5, 7 });

            // Assert
            await page.Received(Quantity.Exactly(6)).DisposeAsync();
        }
    }
}
