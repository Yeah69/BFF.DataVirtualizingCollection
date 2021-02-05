using System.Reactive.Disposables;
using System.Reflection;
using Autofac;
using BFF.DataVirtualizingCollection.Sample.View.ViewModelInterfaceImplementations;
using BFF.DataVirtualizingCollection.Sample.View.Views;
using Module = Autofac.Module;

namespace BFF.DataVirtualizingCollection.Sample.View
{
    public class AutofacModule : Module
    {
        public static MainWindowView Start()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule());
            return builder
                .Build()
                .BeginLifetimeScope()
                .Resolve<MainWindowView>();
        }
        
        private AutofacModule()
        {}
        
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly()
            };

            builder.RegisterAssemblyTypes(assemblies)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterType<GetSchedulers>()
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<CompositeDisposable>()
                .AsSelf()
                .UsingConstructor(() => new CompositeDisposable())
                .InstancePerLifetimeScope();
            
            builder.RegisterModule(new BFF.DataVirtualizingCollection.Sample.ViewModel.AutofacModule());
            
            builder.RegisterModule(new BFF.DataVirtualizingCollection.Sample.Persistence.Proxy.AutofacModule());
        }
    }
}