using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Autofac.Util;
using RegistrationExtensions = Autofac.Integration.WebApi.RegistrationExtensions;

namespace RB.WebApiAutofacFilter
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterWebApiFilterAttribute<TAttribute>(this ContainerBuilder builder, Assembly assembly) where TAttribute : Attribute, IAutofacActionFilter
        {
            Type[] controllerTypes = assembly.GetLoadableTypes()
                .Where(type => typeof(ApiController).IsAssignableFrom(type)).ToArray();
            RegisterFilterForControllers<TAttribute>(builder, controllerTypes);
            RegisterFilterForActions<TAttribute>(builder, controllerTypes);
        }

        // We need to call 
        // builder.RegisterType<TFilter>().AsWebApiActionFilterFor().InstancePerDependency()
        // for each controller marked with TAttribute
        private static void RegisterFilterForControllers<TAttribute>(ContainerBuilder builder, IEnumerable<Type> controllerTypes) where TAttribute : Attribute, IAutofacActionFilter
        {
            foreach (Type controllerType in controllerTypes.Where(c => c.GetCustomAttribute<TAttribute>(false)?.GetType() == typeof(TAttribute)))
            {
                GetAsFilterForControllerMethodInfo(controllerType).Invoke(null, new object[] { builder.RegisterType(typeof(TAttribute)) });
            }
        }

        // We need to call 
        // builder.RegisterType<TFilter>().AsWebApiActionFilterFor(controller => controller.Action(arg)).InstancePerDependency()
        // for each controller action marked with TAttribute
        private static void RegisterFilterForActions<TAttribute>(ContainerBuilder builder, IEnumerable<Type> controllerTypes)
            where TAttribute : Attribute, IAutofacActionFilter
        {
            foreach (Type controllerType in controllerTypes)
            {
                IEnumerable<MethodInfo> actions = controllerType.GetMethods().Where(method => method.IsPublic && method.IsDefined(typeof(TAttribute)));
                foreach (MethodInfo actionMethodInfo in actions)
                {
                    ParameterExpression controllerParameter = Expression.Parameter(controllerType);
                    Expression[] actionMethodArgs = GetActionMethodArgs(actionMethodInfo);
                    GetAsFilterForActionMethodInfo(controllerType).Invoke(null,
                        new object[] {
                            builder.RegisterType(typeof(TAttribute)),
                            Expression.Lambda(typeof(Action<>).MakeGenericType(controllerType), Expression.Call(controllerParameter, actionMethodInfo, actionMethodArgs),
                                controllerParameter)
                        });
                }
            }
        }

        private static Expression[] GetActionMethodArgs(MethodInfo actionMethodInfo)
        {
            return actionMethodInfo.GetParameters().Select(p => Expression.Constant(GetDefaultValueForType(p.ParameterType), p.ParameterType)).ToArray<Expression>();
        }

        /// <summary>
        ///     Returns info for <see cref="Autofac.Integration.WebApi.RegistrationExtensions" />.AsWebApiActionFilterFor()
        ///     method
        /// </summary>
        private static MethodInfo GetAsFilterForControllerMethodInfo(Type controllerType)
        {
            return GetAsFilterForMethodInfo(controllerType, parametersCount: 1);
        }

        /// <summary>
        ///     Returns info for <see cref="Autofac.Integration.WebApi.RegistrationExtensions" />
        ///     .AsWebApiActionFilterFor(actionSelector) method
        /// </summary>
        private static MethodInfo GetAsFilterForActionMethodInfo(Type controllerType)
        {
            return GetAsFilterForMethodInfo(controllerType, parametersCount: 2);
        }

        private static MethodInfo GetAsFilterForMethodInfo(Type controllerType, int parametersCount)
        {
            return typeof(RegistrationExtensions).GetMethods()
                .Single(m => m.Name == nameof(RegistrationExtensions.AsWebApiActionFilterFor) && m.GetParameters().Length == parametersCount)
                .MakeGenericMethod(controllerType);
        }

        private static object GetDefaultValueForType(Type t)
        {
            return typeof(ContainerBuilderExtensions).GetMethod(nameof(GetDefaultGeneric))?.MakeGenericMethod(t).Invoke(null, null);
        }

        private static T GetDefaultGeneric<T>()
        {
            return default;
        }
    }
}