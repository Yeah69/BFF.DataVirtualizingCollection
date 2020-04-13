using System.Reactive.Concurrency;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel.Interfaces
{
    public interface IGetSchedulers
    {
        IScheduler NotificationScheduler { get; }
        
        IScheduler BackgroundScheduler { get; }
    }
}