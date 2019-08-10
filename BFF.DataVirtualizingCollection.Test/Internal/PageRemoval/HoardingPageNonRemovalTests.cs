using System;
using System.Reactive.Subjects;
using BFF.DataVirtualizingCollection.PageRemoval;
using Xunit;
// ReSharper disable UnusedVariable *** Necessary for some tests

namespace BFF.DataVirtualizingCollection.Test.Internal.PageRemoval
{
    public class HoardingPageNonRemovalTests
    {
        [Fact]
        public void Create_RequestAllPagesWithPageSizeOne_NeverRequestsRemoval()
        {
            // Arrange
            var requestedRemoval = false;
            using var pageRequests = new Subject<(int PageKey, int PageIndex)>();

            // Act
            using var subscription = HoardingPageNonRemoval
                .Create()
                (pageRequests)
                .Subscribe(_ => requestedRemoval = true);
            for (var i = 0; i < int.MaxValue; i++)
            {
                pageRequests.OnNext((i, 0));
            }
            pageRequests.OnNext((int.MaxValue, 0));

            // Assert
            Assert.False(requestedRemoval);
        }
    }
}
