using System;
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
    public class PageStorageTests
    {

        [Fact]
        public async Task Index_RequestSeventhPage_SeventhPageRequested()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            using var requests = new ReplaySubject<(int PageKey, int PageIndex)>();
            await using var sut = new PageStorage<int>(
                10,
                10000,
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
            Assert.Collection(requests.ToEnumerable(), tuple =>
            {
                var (pageKey, pageIndex) = tuple;
                Assert.Equal(6, pageKey);
                Assert.Equal(9, pageIndex);
            });
        }

        [Fact]
        public async Task Dispose_ThreePagesRequested_AllThreePagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var sut = new PageStorage<int>(
                10,
                10000,
                (_, __, ___, ____) => page,
                _ => Observable.Never<IReadOnlyList<int>>());

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            await sut.DisposeAsync().ConfigureAwait(false);

            // Assert
            await page.Received(Quantity.Exactly(3)).DisposeAsync();
        }

        [Fact]
        public async Task PageRemoval_RemoveAllExceptFirstRequestedPage_TwoPagesDisposed()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PageStorage<int>(
                10,
                10000,
                (_, __, ___, ____) => page,
                _ => subject);

            // Act
            var i = sut[69];
            var i1 = sut[23];
            var i2 = sut[3];
            subject.OnNext(new []{ 0, 2 });

            // Assert
            await page.Received(Quantity.Exactly(2)).DisposeAsync();
        }

        [Fact]
        public async Task PageCount_Initially0_0()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PageStorageSpy(
                10,
                0,
                (_, __, ___, ____) => page,
                _ => subject);

            // Act
            var result = sut.PageCountDisclosure;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task PageCount_InitiallyCount10000_1000()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PageStorageSpy(
                10,
                10000,
                (_, __, ___, ____) => page,
                _ => subject);

            // Act
            var result = sut.PageCountDisclosure;

            // Assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public async Task PageCount_InitiallyCount0ThenResetCountTo10000_1000()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PageStorageSpy(
                10,
                0,
                (_, __, ___, ____) => page,
                _ => subject);

            await sut.Reset(10000).ConfigureAwait(false);

            // Act
            var result = sut.PageCountDisclosure;

            // Assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public async Task PageCount_InitiallyCount0ThenResetCountTo10001_1001()
        {
            // Arrange
            var page = Substitute.For<IPage<int>>();
            page.ReturnsForAll(69);
            var subject = new Subject<IReadOnlyList<int>>();
            await using var sut = new PageStorageSpy(
                10,
                0,
                (_, __, ___, ____) => page,
                _ => subject);

            await sut.Reset(10001).ConfigureAwait(false);

            // Act
            var result = sut.PageCountDisclosure;

            // Assert
            Assert.Equal(1001, result);
        }

        private class PageStorageSpy : PageStorage<int>
        {
            internal PageStorageSpy(
                int pageSize,
                int count,
                Func<int, int, int, IDisposable, IPage<int>> nonPreloadingPageFactory,
                Func<IObservable<(int PageKey, int PageIndex)>, IObservable<IReadOnlyList<int>>>
                    pageReplacementStrategyFactory)
                : base(pageSize, count, nonPreloadingPageFactory, pageReplacementStrategyFactory) { }

            internal int PageCountDisclosure => PageCount;
        }
    }
}
