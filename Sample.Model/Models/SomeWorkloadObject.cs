namespace BFF.DataVirtualizingCollection.Sample.Model.Models
{
    public interface ISomeWorkloadObject
    {
        int Number { get; }
    }

    internal abstract class SomeWorkloadObject : ISomeWorkloadObject
    {
        public SomeWorkloadObject(int number)
        {
            Number = number;
        }

        public int Number { get; }
    }
}