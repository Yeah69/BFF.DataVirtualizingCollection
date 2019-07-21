using System;
using BFF.DataVirtualizingCollection.PageStorage;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.Internal.PageStorage
{
    public abstract class PageTestsBase
    {
        internal abstract IPage<int> PageWithPageSizeOne { get; }

        [Fact]
        internal void Index_FetchNegativeIndex_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            using var sut = PageWithPageSizeOne;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => sut[-1]);
        }

        [Fact]
        internal void Index_FetchIndexEqualToPageSize_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            using var sut = PageWithPageSizeOne;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => sut[1]);
        }
    }
}
