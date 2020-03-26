using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.SlidingWindow
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    public interface ISlidingWindow<T> :
        IList<T>,
        IReadOnlyList<T>,
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        Task InitializationCompleted { get; }

        void SlideLeft();
        
        void SlideRight();
        
        void JumpTo(int index);
        
        void IncreaseWindowSize();
        
        void DecreaseWindowSize();
        
        void IncreaseWindowSizeBy(int sizeIncrement);
        
        void DecreaseWindowSizeBy(int sizeIncrement);
        
        void Reset();
    }
}
