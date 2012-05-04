using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.UI.Admin;

namespace ContentSync.Controllers
{
    [Admin]
    public class ContentSyncController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly IShapeFactory _shapeFactory;

        public ContentSyncController(IContentManager contentManager,
            IOrchardServices services,
            IShapeFactory shapeFactory) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
        }

        public ActionResult Index() {
            dynamic Shape = _shapeFactory;
            var query = _contentManager.Query(VersionOptions.Latest, "Page").List();

            var list = Shape.List();
            list.AddRange(query.Select(ci => _contentManager.BuildDisplay(ci, "Summary")));

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View((object)viewModel);
        }
    }
}
