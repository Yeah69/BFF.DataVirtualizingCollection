using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BFF.DataVirtualizingCollection
{
    /// <summary>
    /// Root interface of all virtualizing collection types. Implements all necessary .Net interfaces (<c>IList</c>, <c>INotifyCollectionChanged</c>, <c>INotifyPropertyChanged</c>) in order to be usable by standard UI controls (like <c>ItemsControl</c> from WPF). <para/>
    /// Additionally, it has virtualization-specific members of its own. See documentation of the members for further details. <para/>
    /// </summary>
    /// <remarks>
    /// Implements IList in order to mock a standard .Net list. Controls like the <c>ItemsControl</c> from WPF in combination with the <c>VirtualizingStackPanel</c> use the indexer in order to load items on demand. Also the Count-property is used to arrange the scroll bar accordingly. <para/>
    /// The virtualized collections automatically changes its items. For example, if an async index access or preloading is set up, it will be eventually necessary to replace placeholders with the actually loaded items. In order to notify this changes to the UI the <c>INotifyCollectionChanged</c> interface is implemented. <para/>
    /// Also the properties of the collection itself may change, especially the Count-property. Hence notifications for such changes are send with <c>INotifyPropertyChanged</c>. <para/>
    /// Finally, each virtualizing collection has to be disposed after use, because it uses Rx subscriptions which have to be cleaned up. Also the currently active pages including their items which implement <c>IDisposable</c> are disposed as well. 
    /// </remarks>
    public interface IVirtualizationBase :
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDisposable
    {
        /// <summary>
        /// Task is successfully completed when initialization is completed. <para/>
        /// </summary>
        /// <remarks>
        /// Initialization depends on the initial calculation of the Count-property. Because of the asynchronicity of task-based fetchers the Count-property might not be calculated at the end of construction of the virtualized collection.
        /// </remarks>
        Task InitializationCompleted { get; }
        
        /// <summary>
        /// Can be bound to SelectedIndexProperty on Selector controls in order to workaround issue with resets and selected items. <para/>
        /// </summary>
        /// <remarks>
        /// In WPF the Selector control will search for the previously selected item after each reset by iterating over all items until found. This behavior is the opposite of virtualization. Hence, the virtualizing collection would set this property to -1 (deselection) and notify the change before notifying any reset.
        /// </remarks>
        int SelectedIndex { get; set; }

        /// <summary>
        /// Disposes of all current pages and notifies that possibly everything changed. <para/>
        /// The Reset-function should be called any time when something in the virtualized backend has changed. <para/>
        /// </summary>
        /// <remarks>
        /// Consequently, the Count-property is recalculated. The UI will request all currently rendered items anew, so this items get reloaded.
        /// </remarks>
        void Reset();
    }
    
    /// <summary>
    /// The generic version of <see cref="IVirtualizationBase"/>. Analogously, it implements <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity *** the "new" members of this interface resolve the ambiguities
    public interface IVirtualizationBase<T> :
        IVirtualizationBase,
        IList<T>,
        IReadOnlyList<T>
    {
        /// <summary>
        /// The Count-property is newed here in order to resolve ambiguities cause by implementing <see cref="IList"/>, <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/> at the same time.
        /// </summary>
        new int Count { get; }
        
        /// <summary>
        /// The indexer is newed here in order to resolve ambiguities cause by implementing <see cref="IList"/>, <see cref="IList{T}"/> and <see cref="IReadOnlyList{T}"/> at the same time.
        /// </summary>
        new T this[int index] { get; }
    }
}