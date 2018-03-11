using Autofac.Integration.WebApi;
using Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace RB.WebApiAutofacFilter.Logging
{
    public class LoggerFilterAttribute : Attribute, IAutofacActionFilter
    {
        private readonly ILog _logger;

        public LoggerFilterAttribute() { }
        public LoggerFilterAttribute(ILog logger)
        {
            _logger = logger;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            HttpActionContext actionContext = actionExecutedContext.ActionContext;
            _logger.Trace($"Action '{actionContext.ControllerContext.ControllerDescriptor.ControllerName}.{actionExecutedContext.ActionContext.ActionDescriptor.ActionName}' is executed");
            return Task.CompletedTask;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            _logger.Trace($"Action '{actionContext.ControllerContext.ControllerDescriptor.ControllerName}.{actionContext.ActionDescriptor.ActionName}' is started");
            return Task.CompletedTask;
        }
    }
}