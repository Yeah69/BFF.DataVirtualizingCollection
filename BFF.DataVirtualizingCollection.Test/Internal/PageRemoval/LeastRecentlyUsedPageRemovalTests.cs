using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.PageRemoval;
using BFF.DataVirtualizingCollection.Utilities;
using NSubstitute;
using Xunit;
// ReSharper disable UnusedVariable *** Necessary for some tests

namespace BFF.DataVirtualizingCollection.Test.Internal.PageRemoval
{
    public class LeastRecentlyUsedPageRemovalTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount1_Request10Pages_NeverRequestsRemoval(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    1,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 0; i < 10; i++)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Empty(requestedRemovals);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount1_Request10PagesAndIterateThemAgain_NeverRequestsRemoval(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    1,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 0; i < 10; i++)
            {
                pageRequests.OnNext((i, 0));
            }
            for (var i = 9; i >= 0; i--)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Empty(requestedRemovals);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount1_Request11Pages_FirstPageRemovalRequest(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    1,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 0; i < 11; i++)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Collection(
                requestedRemovals,
                pageKey => Assert.Equal(0, pageKey));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount1_Request11PagesInReverse_FirstPageRemovalRequest(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    1,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 10; i >= 0; i--)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Collection(
                requestedRemovals,
                pageKey => Assert.Equal(10, pageKey));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount3_Request11Pages_FirstThreePagesRemovalRequest(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    3,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 0; i < 11; i++)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Collection(
                requestedRemovals,
                pageKey => Assert.Equal(0, pageKey),
                pageKey => Assert.Equal(1, pageKey),
                pageKey => Assert.Equal(2, pageKey));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreatePageLimit10RemovalCount3_Request11PagesInReverse_FirstThreePagesRemovalRequest(bool isPreloading)
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    10,
                    3,
                    isPreloading,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 10; i >= 0; i--)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Collection(
                requestedRemovals,
                pageKey => Assert.Equal(10, pageKey),
                pageKey => Assert.Equal(9, pageKey),
                pageKey => Assert.Equal(8, pageKey));
        }

        [Fact]
        public void CreatePageLimit1RemovalCount1Preloading_Request5Pages_FirstPageRemovalRequest()
        {
            // Arrange
            var timestampCount = 0;
            var timestampProvider = Substitute.For<ITimestampProvider>();
            timestampProvider.Now.Returns(_ => DateTime.MinValue + TimeSpan.FromTicks(1) * timestampCount++);
            var requestedRemovals = new List<int>();
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = LeastRecentlyUsedPageRemoval
                .Create(
                    1,
                    1,
                    true,
                    timestampProvider)
                (pageRequests)
                .Subscribe(removals => requestedRemovals.AddRange(removals));
            for (var i = 0; i < 5; i++)
            {
                pageRequests.OnNext((i, 0));
            }

            // Assert
            Assert.Collection(
                requestedRemovals,
                pageKey => Assert.Equal(0, pageKey));
        }
    }
}
