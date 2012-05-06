using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using ContentSync.Extensions;
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
        private readonly IRemoteSyncService _remoteSyncService;

        public ContentSyncController(IContentManager contentManager,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager,
            IRemoteSyncService remoteSyncService) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _remoteSyncService = remoteSyncService;
        }

        public ActionResult Index() {
            return View();
        }

        public ActionResult Prepare(string remote) {
            // get remote content
            var remoteContent = _remoteSyncService.Fetch(new Uri(remote))
                .Where(rci=>rci.Has<IdentityPart>())
                .ToList();

            dynamic Shape = _shapeFactory;
            var query = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            var list = Shape.List();
            list.AddRange(query.Select(ci => _contentManager.BuildDisplay(ci, "Summary")));

            List<ContentSyncMap> mappings = new List<ContentSyncMap>();

            foreach(var localItem in query) {
                if (!localItem.Has<IdentityPart>())
                    continue;

                ContentSyncMap map = new ContentSyncMap();
                map.Local = new ContentItemSyncInfo(localItem, _contentManager.BuildDisplay(localItem, "Summary"));
                map.Identifier = localItem.As<IdentityPart>().Identifier;
                // try to find a match
                for (int i = 0; i < remoteContent.Count;i++ ) {
                    var remoteItem = remoteContent[i];
                    if (localItem.IsEqualTo(remoteItem))
                    {
                        map.Remote = new ContentItemSyncInfo(remoteItem, _contentManager.BuildDisplay(remoteItem, "Summary"));
                        remoteContent.Remove(remoteItem);
                        break;
                    }
                }

                if (map.Remote ==null) {
                    map.Similar = remoteContent.Where(r => map.Local.ContentItem.SimilarTo(r))
                        .Select(r=>new ContentItemSyncInfo(r, _contentManager.BuildDisplay(r,"Summary")));
                }

                mappings.Add(map);
            }

            foreach (var remoteContentItem in remoteContent)
            {
                mappings.Add(new ContentSyncMap() {
                    Remote=new ContentItemSyncInfo(remoteContentItem, _contentManager.BuildDisplay(remoteContentItem, "Summary")),
                    Identifier = remoteContentItem.As<IdentityPart>().Identifier
                });
            }

            return View(mappings);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

    }
}
