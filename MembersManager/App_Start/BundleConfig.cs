using System.Web;
using System.Web.Optimization;

namespace MembersManager
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/vendor/bootstrap/js/bootstrap.bundle.min.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/plugins").Include(
                        "~/vendor/jquery-easing/jquery.easing.min.js",
                        "~/vendor/datatables/jquery.dataTables.js",
                        "~/vendor/datatables/dataTables.bootstrap4.js"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/customJS").Include(
                        "~/js/sb-admin.min.js",
                        "~/js/sb-admin-datatables.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/vendor/bootstrap/css/bootstrap.min.css",
                        "~/vendor/font-awesome/css/font-awesome.min.css",
                        "~/vendor/datatables/dataTables.bootstrap4.css",
                        "~/css/sb-admin.css",
                        "~/Content/site.css"));
        }
    }
}
