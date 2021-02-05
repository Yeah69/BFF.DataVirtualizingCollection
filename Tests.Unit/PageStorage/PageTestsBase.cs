using System;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.PageStorage;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test.PageStorage
{
    public abstract class PageTestsBase
    {
        internal abstract IPage<int> PageWithPageSizeOne { get; }

        [Fact]
        internal async Task Index_FetchNegativeIndex_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            await using var sut = PageWithPageSizeOne;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => sut[-1]);
        }

        [Fact]
        internal async Task Index_FetchIndexEqualToPageSize_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            await using var sut = PageWithPageSizeOne;

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => sut[1]);
        }
    }
}
