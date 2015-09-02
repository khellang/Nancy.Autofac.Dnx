using System;
using Autofac;
using Autofac.Framework.DependencyInjection;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Nancy;
using Nancy.Owin;

namespace WebApplication1
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(); // This adds ILoggerFactory and ILogger which I'll use in the Nancy module as an example.

            var builder = new ContainerBuilder();

            // Populate the builder using Autofac.Framework.DependencyInjection...
            // This will make sure all core services from DNX as well as services added using
            // extension methods on IServiceCollection will be added to the container.
            builder.Populate(services);

            // Keep a reference to our pre-built container
            // so we can pass it to the Nancy bootstrapper.
            Container = builder.Build();

            Container.Resolve<ILoggerFactory>().AddConsole(LogLevel.Information);

            // Return the proper IServiceProvider.
            return Container.Resolve<IServiceProvider>();
        }

        private IContainer Container { get; set; }

        public void Configure(IApplicationBuilder app)
        {
            // Make Nancy use bootstrapper with pre-built container.
            app.UseOwin(owin =>
                owin.UseNancy(options =>
                    options.Bootstrapper = new Bootstrapper(Container)));
        }

        private class Bootstrapper : AutofacNancyBootstrapper
        {
            private readonly ILifetimeScope _container;

            public Bootstrapper(ILifetimeScope container)
            {
                _container = container;
            }

            protected override ILifetimeScope GetApplicationContainer()
            {
                return _container; // We'll just return the pre-built container here...
            }
        }
    }

    public class HelloWorldModule : NancyModule
    {
        // Here we've automatically gotten an ILogger injected by DNX and our Autofac container. Weee! :)
        public HelloWorldModule(ILogger<HelloWorldModule> logger)
        {
            Get["/"] = _ =>
            {
                logger.LogInformation("Hello World!");
                return "Hello World!";
            };
        }
    }
}
