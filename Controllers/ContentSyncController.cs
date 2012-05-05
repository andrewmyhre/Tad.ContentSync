using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ContentSync.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
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
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentSyncController(IContentManager contentManager,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public ActionResult Index() {
            dynamic Shape = _shapeFactory;
            var query = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            var list = Shape.List();
            list.AddRange(query.Select(ci => _contentManager.BuildDisplay(ci, "Summary")));

            var count = query.Count();

            List<ContentSyncMap> syncMap = query.Select(ci=>new ContentSyncMap() {
                Local = new ContentItemSyncInfo() {
                    ContentItem = ci,
                    Shape=_contentManager.BuildDisplay(ci, "Summary")
                },
                Identifier = ci.Has<IdentityPart>() ? ci.As<IdentityPart>().Identifier : ""
            }).ToList();

            return View(syncMap);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

    }
}
