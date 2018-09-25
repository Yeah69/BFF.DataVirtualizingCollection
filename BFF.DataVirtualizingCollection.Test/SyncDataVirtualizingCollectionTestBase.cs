using System;
using System.Collections.Generic;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using Xunit;

namespace BFF.DataVirtualizingCollection.Test
{
    public abstract class SyncDataVirtualizingCollectionTestBase
    {
        [Fact]
        public void BuildingCollection0To6969_FirstEntry_0()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();
            
            // Act + Assert
            Assert.Equal(0, ((IList<int>)collection)[0]);
        }

        [Fact]
        public void BuildingCollection0To6969_70thEntry_69()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Equal(69, ((IList<int>)collection)[69]);
        }

        [Fact]
        public void BuildingCollection0To6969_124thEntry_123()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Equal(123, ((IList<int>)collection)[123]);
        }

        [Fact]
        public void BuildingCollection0To6969_6001thEntry_6000()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Equal(6000, ((IList<int>)collection)[6000]);
        }

        [Fact]
        public void BuildingCollection0To6969_6969thEntry_6968()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Equal(6968, ((IList<int>)collection)[6968]);
        }

        [Fact]
        public void BuildingCollection0To6969_6970thEntry_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[6969]);
        }

        [Fact]
        public void BuildingCollection0To6969_MinusFirstEntry_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var collection = GenerateCollectionToBeTested();

            // Act + Assert
            Assert.Throws<IndexOutOfRangeException>(() => ((IList<int>)collection)[6969]);
        }

        [Fact]
        public void BuildingCollectionWherePageFetcherIgnoresGivenPageSize23_70thEntry_69()
        {
            // Arrange
            var collection = GenerateCollectionWherePageFetcherIgnoresGivenPageSize(pageSize: 23);

            // Act + Assert
            Assert.Equal(69, ((IList<int>)collection)[69]);
        }

        protected abstract IDataVirtualizingCollection<int> GenerateCollectionToBeTested(int count = 6969, int pageSize = 100);

        protected abstract IDataVirtualizingCollection<int> GenerateCollectionWherePageFetcherIgnoresGivenPageSize(
            int count = 6969, int pageSize = 100);
    }
}
