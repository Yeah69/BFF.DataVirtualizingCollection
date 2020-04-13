using System;

namespace BFF.DataVirtualizingCollection.Sample.Model.Models
{
    public interface IHighWorkloadObject : ISomeWorkloadObject
    {
    }

    internal class HighWorkloadObject : SomeWorkloadObject, IHighWorkloadObject, IDisposable
    {
        // Simulates workload
        // ReSharper disable once UnusedMember.Local
        private readonly byte[] _workload = new byte[12500];

        public HighWorkloadObject(int number) : base(number)
        {
        }

        public void Dispose()
        {
            Console.WriteLine("Disposed");
        }
    }
}