using System.Windows;
using System.Windows.Data;

namespace BFF.DataVirtualizingCollection.Sample.View.Utilities
{
    public static class Converters
    {
        public static IValueConverter BoolToVisibility =
            LambdaConverters.ValueConverter.Create<bool, Visibility>(
                e => e.Value ? Visibility.Visible : Visibility.Collapsed);

        public static IValueConverter ValueEqualsToParameter =
            LambdaConverters.ValueConverter.Create<object, bool, object>(
                e => Equals(e.Value, e.Parameter));
    }
}
