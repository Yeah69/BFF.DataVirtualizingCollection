namespace BFF.DataVirtualizingCollection.Sample.Model.Models
{
    public interface ILowWorkloadObject : ISomeWorkloadObject
    {
    }

    internal class LowWorkloadObject : SomeWorkloadObject, ILowWorkloadObject
    {
        public LowWorkloadObject(int number) : base(number)
        {
        }
    }
}