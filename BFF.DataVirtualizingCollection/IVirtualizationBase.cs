using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection
{
    public interface IVirtualizationBase :
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        /// <summary>
        /// Task is successfully completed when initialization is completed
        /// </summary>
        Task InitializationCompleted { get; }
        
        /// <summary>
        /// Can be bound to SelectedIndexProperty on Selector Controls in order to workaround issue with resets and selected items.
        /// </summary>
        int SelectedIndex { get; set; }

        /// <summary>
        /// Disposes of all current pages and notifies that possibly everything changed.
        /// </summary>
        void Reset();
    }
    
    public interface IVirtualizationBase<T> :
        IVirtualizationBase,
        IList<T>,
        IReadOnlyList<T>
    {
        new int Count { get; }
    }
}