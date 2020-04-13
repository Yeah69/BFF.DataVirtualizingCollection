using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace BFF.DataVirtualizingCollection.Sample.Persistence
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly()
            };

            builder.RegisterAssemblyTypes(assemblies)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}