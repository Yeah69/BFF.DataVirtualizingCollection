using Autofac;
using Module = Autofac.Module;

namespace BFF.DataVirtualizingCollection.Sample.Persistence.Proxy
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new BFF.DataVirtualizingCollection.Sample.Persistence.AutofacModule());
        }
    }
}