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
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Teams.Apps.Common.Configuration;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;
    using Microsoft.Teams.Apps.ListSearch.Configuration.Controllers;

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
                return new TelemetryClient(new TelemetryConfiguration(ConfigurationManager.AppSettings["ApplicationInsightsInstrumentationKey"]));
            }).SingleInstance();

            var config = new LocalConfigProvider();

            builder.Register(c => config)
                .As<IConfigProvider>()
                .SingleInstance();

            builder.Register(c => new AppInsightsLogProvider(c.Resolve<IConfigProvider>()))
               .As<ILogProvider>()
               .SingleInstance();

            builder.Register(c => new HttpClient())
                .SingleInstance();

            builder.Register(c => new KBInfoHelper(ConfigurationManager.AppSettings["StorageConnectionString"]))
                .SingleInstance();

            builder.Register(c => new TokenHelper(
                c.Resolve<HttpClient>(),
                ConfigurationManager.AppSettings["StorageConnectionString"],
                ConfigurationManager.AppSettings["ida:TenantId"],
                ConfigurationManager.AppSettings["GraphAppClientId"],
                ConfigurationManager.AppSettings["GraphAppClientSecret"],
                ConfigurationManager.AppSettings["TokenEncryptionKey"]))
                .SingleInstance();

            builder.Register(c => new GraphHelper(
                c.Resolve<HttpClient>(),
                c.Resolve<TokenHelper>()))
                .SingleInstance();

            builder.Register(c => new QnAMakerService(
                c.Resolve<HttpClient>(),
                ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"]))
                .SingleInstance();

            builder.Register(c => new BlobHelper(
                ConfigurationManager.AppSettings["StorageConnectionString"]))
                .SingleInstance();

            builder.Register(c => new KnowledgeBaseRefreshHelper(
                c.Resolve<HttpClient>(),
                c.Resolve<BlobHelper>(),
                c.Resolve<KBInfoHelper>(),
                c.Resolve<GraphHelper>(),
                ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"],
                c.Resolve<ILogProvider>()))
                .SingleInstance();

            builder.RegisterType<HomeController>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}