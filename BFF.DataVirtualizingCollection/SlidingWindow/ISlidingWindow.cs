using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.Window
{
    public interface ISlidingWindow<T> :
        IList<T>,
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        Task InitializationCompleted { get; }

        void SlideLeft();
        void SlideRight();
        void Jump(long index);

    }
}
