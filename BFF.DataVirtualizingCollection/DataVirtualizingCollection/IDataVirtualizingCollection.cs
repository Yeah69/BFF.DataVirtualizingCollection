using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection.DataVirtualizingCollection
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    // Ambiguous Members should be implemented explicitly
    /// <summary>
    /// Defines a data virtualizing collection.
    /// The IList interfaces are necessary for offering an indexer to access the data.
    /// The notification interfaces can be used in order to notify the UI of certain changes (such as replacement of a placeholder).
    /// </summary>
    /// <typeparam name="T">Type of the collection items.</typeparam>
    public interface IDataVirtualizingCollection<T> :
        IList<T>,
        IList,
        IReadOnlyList<T>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        /// <summary>
        /// Task is successfully completed when initialization is completed
        /// </summary>
        Task InitializationCompleted { get; }
    }
}