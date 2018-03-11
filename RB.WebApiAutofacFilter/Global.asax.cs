using Autofac;
using Autofac.Integration.WebApi;
using RB.WebApiAutofacFilter.Controllers;
using System.Reflection;
using System.Web.Http;
using RB.WebApiAutofacFilter.Logging;
using Common.Logging;

namespace RB.WebApiAutofacFilter
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            RegisterAutofac(GlobalConfiguration.Configuration);
        }

        private static void RegisterAutofac(HttpConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(configuration);
            builder.Register(c => LogManager.GetLogger("LoggerResolvedWithAutofac")).As<ILog>().InstancePerLifetimeScope();
            builder.RegisterWebApiFilterAttribute<LoggerFilterAttribute>(Assembly.GetExecutingAssembly());

            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
        }
    }
}
