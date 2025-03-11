﻿using System.Web.Optimization;

public class BundleConfig
{
    public static void RegisterBundles(BundleCollection bundles)
    {
        bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-3.6.0.min.js"));

        bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                    "~/Scripts/jquery-ui-3.6.0.min.js"));

        bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.unobtrusive-ajax.js",
                  "~/Scripts/jquery.validate.js",
                   "~/Scripts/jquery.validate.unobtrusive.js",
                    "~/Commons_JS/Common.js"
                  ));

        bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                    "~/Scripts/modernizr-*"));

        bundles.Add(new ScriptBundle("~/bundles/JS").Include(
                         "~/js/jquery-3.6.0.min.js",
                         "~/js/popper.min.js",
                         "~/js/bootstrap.min.js",
                         "~/js/main.js",
                         "~/js/plugins/pace.min.js"
                     ));

        bundles.Add(new StyleBundle("~/bootstrap/css").Include(
                  "~/css/bootstrap.min.css"
                  ));

        bundles.Add(new StyleBundle("~/Content/fontawesome").Include(
          "~/Content/font-awesome/css/font-awesome.min.css"));
    }
}
