using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ContentSync.Models;
using ContentSync.Services;
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
        private readonly IRemoteImportService _remoteImportService;

        public ContentSyncController(IContentManager contentManager,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager,
            IRemoteImportService remoteImportService) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _remoteImportService = remoteImportService;
        }

        public ActionResult Index() {
            // get remote content
            var remoteContent = _remoteImportService.Fetch(new Uri("http://www.local.orchard.com"));




            dynamic Shape = _shapeFactory;
            var query = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            var list = Shape.List();
            list.AddRange(query.Select(ci => _contentManager.BuildDisplay(ci, "Summary")));

            var count = query.Count();
            var localAndRemote = query
                .Where(ci => remoteContent.Any(remote => remote.As<IdentityPart>().Identifier.Equals(ci.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ci => new ContentSyncMap() {
                    Local = new ContentItemSyncInfo() {
                        ContentItem = ci,
                        Shape = _contentManager.BuildDisplay(ci, "Summary"),

                    },
                    Remote = new ContentItemSyncInfo() {
                        ContentItem = remoteContent.SingleOrDefault(c => c.As<IdentityPart>().Identifier == ci.As<IdentityPart>().Identifier),
                        Shape = _contentManager.BuildDisplay(remoteContent.SingleOrDefault(c => c.As<IdentityPart>().Identifier == ci.As<IdentityPart>().Identifier),
                                                             "Summary")
                    },
                    Identifier = ci.Has<IdentityPart>() ? ci.As<IdentityPart>().Identifier : ""
                });

            var localOnly = query
                .Where(ci => !remoteContent.Any(remote => remote.As<IdentityPart>().Identifier.Equals(ci.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase)))
                .Select(ci => new ContentSyncMap() {
                    Local = new ContentItemSyncInfo() {
                        ContentItem = ci,
                        Shape = _contentManager.BuildDisplay(ci, "Summary"),

                    },
                    Identifier = ci.Has<IdentityPart>() ? ci.As<IdentityPart>().Identifier : ""
                });

            var remoteOnly = remoteContent
                .Where(remote => !query.Any(local => local.As<IdentityPart>().Identifier.Equals(remote.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase)))
                .Select(remote => new ContentSyncMap() {
                    Remote = new ContentItemSyncInfo() {
                        ContentItem = remote,
                        Shape = _contentManager.BuildDisplay(remote, "Summary")
                    },
                    Identifier = remote.As<IdentityPart>().Identifier
                });

            var syncMap = localAndRemote.Union(localOnly).Union(remoteOnly).ToList();

            return View(syncMap);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

    }
}
