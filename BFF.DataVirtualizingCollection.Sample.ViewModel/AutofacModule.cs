using System.Reflection;
using Autofac;
using Module = Autofac.Module;

namespace BFF.DataVirtualizingCollection.Sample.ViewModel
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
            
            builder.RegisterModule(new BFF.DataVirtualizingCollection.Sample.Model.AutofacModule());
        }
    }
}