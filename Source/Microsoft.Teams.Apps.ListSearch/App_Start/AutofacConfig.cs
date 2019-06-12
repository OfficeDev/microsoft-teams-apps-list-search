// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch
{
    using System.Configuration;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;
    using Microsoft.Teams.Apps.Common.Configuration;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.ListSearch.Common.Helpers;

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
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var config = new LocalConfigProvider();

            builder.Register(c => config)
                .As<IConfigProvider>()
                .SingleInstance();

            builder.Register(c => new AppInsightsLogProvider(c.Resolve<IConfigProvider>()))
                .As<ILogProvider>()
                .SingleInstance();

            builder.Register(c => new HttpClient())
                .SingleInstance();

            builder.Register(c => new JwtHelper(
                jwtSecurityKey: ConfigurationManager.AppSettings["TokenEncryptionKey"],
                botId: ConfigurationManager.AppSettings["MicrosoftAppId"]))
                .SingleInstance();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}