using System.Windows;
using System.Windows.Data;

namespace BFF.DataVirtualizingCollection.Sample
{
    public static class Converters
    {
        public static IValueConverter BoolToVisibility =
            LambdaConverters.ValueConverter.Create<bool, Visibility>(
                e => e.Value ? Visibility.Visible : Visibility.Collapsed);
    }
}
