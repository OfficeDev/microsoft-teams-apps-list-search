// <copyright file="BundleConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ListSearch
{
    using System.Web.Optimization;

    /// <summary>
    /// Bundle config for Task Module app.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        /// </summary>
        /// <param name="bundles">Collection of bundles</param>
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/lib/jquery/jquery.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/lib/modernizr/modernizr.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/lib/bootstrap/dist/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/msteams").Include(
                      "~/lib/microsoft-teams/dist/MicrosoftTeams.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/lib/bootstrap/dist/css/bootstrap.min.css",
                "~/lib/msteams-ui-styles-core/css/msteams-10.css",
                "~/Content/spinner.css",
                "~/Content/customSite.css"));
        }
    }
}
