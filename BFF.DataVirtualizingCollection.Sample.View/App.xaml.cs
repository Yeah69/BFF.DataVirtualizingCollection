using System.Windows;

namespace BFF.DataVirtualizingCollection.Sample.View
{
    public partial class App
    {
        public App()
        {
            var mainWindowView = AutofacModule.Start();
            mainWindowView.Show();
        }

        public static Visibility IsDebug
        {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }
    }
}