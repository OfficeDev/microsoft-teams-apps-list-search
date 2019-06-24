// <copyright file="RouteConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.ListSearch
{
    using System.Web.Mvc;
    using System.Web.Routing;

    /// <summary>
    /// Route Config for Task Module App
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Register routes for the app.
        /// </summary>
        /// <param name="routes">Collection of routes.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Search", action = "Index", id = UrlParameter.Optional });
        }
    }
}
