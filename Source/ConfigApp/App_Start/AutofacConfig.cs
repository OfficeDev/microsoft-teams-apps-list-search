// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ConfigApp
{
    using System.Configuration;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using ConfigApp.Controllers;
    using Lib.Helpers;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// Autofac configuration
    /// </summary>
    public class AutofacConfig
    {
        /// <summary>
        /// Register Autofac dependencies
        /// </summary>
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(c =>
            {
                return new TelemetryClient(new TelemetryConfiguration(ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"]));
            }).SingleInstance();

            builder.Register(c => new HttpClient())
                .As<HttpClient>()
                .SingleInstance();

            builder.Register(c => new TokenHelper(
                c.Resolve<HttpClient>(),
                ConfigurationManager.AppSettings["StorageConnectionString"],
                ConfigurationManager.AppSettings["ida:TenantId"],
                ConfigurationManager.AppSettings["GraphAppClientId"],
                ConfigurationManager.AppSettings["GraphAppClientSecret"],
                ConfigurationManager.AppSettings["TokenKey"]))
                .As<TokenHelper>()
                .SingleInstance();

            builder.RegisterType<HomeController>().InstancePerRequest();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));
        }
    }
}