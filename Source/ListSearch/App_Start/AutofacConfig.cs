// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch.App_Start
{
    using System.Configuration;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;
    using Lib.Helpers;
    using ListSearch.Controllers;
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
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.Register(c =>
            {
                return new TelemetryClient(new TelemetryConfiguration(ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"]));
            }).SingleInstance();

            builder.Register(c => new HttpClient()).As<HttpClient>().SingleInstance();
            builder.Register(c => new JwtHelper(
                jwtSecurityKey: ConfigurationManager.AppSettings["JWTSecurityKey"],
                botId: ConfigurationManager.AppSettings["MicrosoftAppId"])).As<JwtHelper>().SingleInstance();

            builder.RegisterType<SearchController>().InstancePerRequest();
            builder.RegisterType<RefreshController>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}