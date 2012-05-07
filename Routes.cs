using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Tad.ContentSync
{
    public class Routes : IRouteProvider
    { 
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new RouteDescriptor[] {
                SimpleRoute("Admin/ContentSync/{action}", "Tad.ContentSync", "ContentSync", "Index"),
                SimpleRoute("Admin/ContentImportExport/{action}", "Tad.ContentSync", "ContentImportExport", "Index"),
            };
        }

        private static RouteDescriptor SimpleRoute(string url, object area, object controller, object defaultAction) {
            return new RouteDescriptor() {
                Route = new Route(url,
                                  new RouteValueDictionary {{"area", area}, {"action", defaultAction}, {"controller", controller}},
                                  new RouteValueDictionary(),
                                  new RouteValueDictionary() {{"area", area}},
                                  new MvcRouteHandler())
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}
