// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch.Configuration
{
    using System.Configuration;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Configuration.Controllers;
    using ListSearch.Common.Helpers;
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
        /// <returns>Autofac container</returns>
        public static IContainer RegisterDependencies()
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

            builder.Register(c => new KBInfoHelper(ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<KBInfoHelper>()
                .SingleInstance();

            builder.Register(c => new TokenHelper(
                c.Resolve<HttpClient>(),
                ConfigurationManager.AppSettings["StorageConnectionString"],
                ConfigurationManager.AppSettings["ida:TenantId"],
                ConfigurationManager.AppSettings["GraphAppClientId"],
                ConfigurationManager.AppSettings["GraphAppClientSecret"],
                ConfigurationManager.AppSettings["TokenEncryptionKey"]))
                .As<TokenHelper>()
                .SingleInstance();

            builder.RegisterType<HomeController>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}