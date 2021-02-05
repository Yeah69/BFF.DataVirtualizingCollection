using System.Windows;
using System.Windows.Controls;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Adapters;
using BFF.DataVirtualizingCollection.Sample.ViewModel.ViewModels;

namespace BFF.DataVirtualizingCollection.Sample.View.Utilities
{
    public class BackendAccessDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AllNumbersTemplate { get; set; }
        public DataTemplate? HighWorkloadTemplate { get; set; }
        public DataTemplate? MillionNumbersTemplate { get; set; }
        public DataTemplate? ProfileTemplate { get; set; }
        
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item is IDataVirtualizingCollectionViewModelBase dataVirtualizingCollectionViewModel
                ? dataVirtualizingCollectionViewModel.BackendAccessKind switch
                {
                    BackendAccessKind.AllNumbers => AllNumbersTemplate,
                    BackendAccessKind.HighWorkload => HighWorkloadTemplate,
                    BackendAccessKind.MillionNumbers => MillionNumbersTemplate,
                    BackendAccessKind.Profiles => ProfileTemplate,
                    _ => base.SelectTemplate(item, container)
                }
                : base.SelectTemplate(item, container);
        }
    }
}