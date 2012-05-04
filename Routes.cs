using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace ContentSync
{
    public class Routes : IRouteProvider
    {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new RouteDescriptor[] {
                new RouteDescriptor() {
                    Route=new Route("Admin/ContentSync/{action}",
                        new RouteValueDictionary{{"area","ContentSync"}, {"action","Index"}, {"controller","ContentSync"}},
                        new RouteValueDictionary(),
                        new RouteValueDictionary(){{"area","ContentSync"}},
                        new MvcRouteHandler())
                }
            };
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }
    }
}
