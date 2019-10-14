using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.PageStorage;
using NSubstitute;
using NSubstitute.Extensions;
using NSubstitute.ReceivedExtensions;
using Xunit;
// ReSharper disable UnusedVariable *** Necessary for some tests
// ReSharper disable AccessToDisposedClosure *** Controlled disposal

namespace BFF.DataVirtualizingCollection.Test.Internal.PageStorage
{
    public class PreloadingPageStorageTests
    {
        [Fact]
        public void Index_RequestSeventhPage_SeventhSixthAndEighthPageRequested()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            using var requests = new ReplaySubject<(int PageKey, int PageIndex)>();
            using var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___) => page,
                (_, __, ___) => page,
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
                    Assert.Equal(5, pageKey);
                    Assert.Equal(-1, pageIndex);
                },
                tuple =>
                {
                    var (pageKey, pageIndex) = tuple;
                    Assert.Equal(7, pageKey);
                    Assert.Equal(-1, pageIndex);
                });
        }

        [Fact]
        public void Dispose_ThreePagesRequested_SevenPagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___) => page,
                (_, __, ___) => page,
                _ => Observable.Never<IReadOnlyList<int>>());

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            sut.Dispose();

            // Assert
            page.Received(Quantity.Exactly(7)).Dispose();
        }

        [Fact]
        public void PageRemoval_RemoveAllExceptFirstRequestedPage_SixPagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            using var sut = new PreloadingPageStorage<int>(
                10,
                10000,
                (_, __, ___) => page,
                (_, __, ___) => page,
                _ => subject);

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            subject.OnNext(new[] { 0, 1, 2, 3, 5, 7 });

            // Assert
            page.Received(Quantity.Exactly(6)).Dispose();
        }
    }
}
