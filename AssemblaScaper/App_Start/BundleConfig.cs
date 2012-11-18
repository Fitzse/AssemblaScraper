using System.Web.Optimization;

namespace AssemblaScaper.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts/foundation").Include(
                "~/Scripts/foundation/foundation.min.js",
                "~/Scripts/foundation/jquery.foundation.accordion.js",
                "~/Scripts/foundation/modernizr.foundation.js"));

            bundles.Add(new StyleBundle("~/bundles/styles/foundation").IncludeDirectory(
                "~/Content/foundation", "*.css"));
        }
    }
}