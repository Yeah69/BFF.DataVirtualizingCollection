using BFF.DataVirtualizingCollection.Window;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollections
{
    internal abstract class SlidingWindowBase<T> : VirtualizationBase<T>, ISlidingWindow<T>
    {
        public void SlideLeft()
        {
            throw new System.NotImplementedException();
        }

        public void SlideRight()
        {
            throw new System.NotImplementedException();
        }

        public void Jump(long index)
        {
            throw new System.NotImplementedException();
        }
    }
}