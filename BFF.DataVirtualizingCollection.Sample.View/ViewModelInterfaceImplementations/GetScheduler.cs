using System.Reactive.Concurrency;
using System.Windows;
using BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces;

namespace BFF.DataVirtualizingCollection.Sample.View.ViewModelInterfaceImplementations
{
    internal class GetSchedulers : IGetSchedulers
    {
        public GetSchedulers()
        {
            NotificationScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
        }
        
       public IScheduler NotificationScheduler { get; }
       public IScheduler BackgroundScheduler => TaskPoolScheduler.Default;
    }
}