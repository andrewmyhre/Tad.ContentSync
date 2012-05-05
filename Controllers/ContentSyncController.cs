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
using Orchard.Core.Title.Models;
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
            var remoteContent = _remoteImportService.Fetch(new Uri("http://www.local.orchard.com"))
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
                    if (localItem.IsEqualTo(remoteItem, _contentManager))
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

    public static class ContentItemExtensions {
        public static bool IsEqualTo(this ContentItem o1, ContentItem o2, IContentManager contentManager) {
            //todo: this is a little too generous

            if(o1.Has<IdentityPart>() && o2.Has<IdentityPart>()) {
                if (!o1.As<IdentityPart>().Identifier.Equals(o2.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if(o1.Has<TitlePart>() && o2.Has<TitlePart>()) {
                if (!o1.As<TitlePart>().Title.Equals(o2.As<TitlePart>().Title, StringComparison.CurrentCulture)) {
                    return false;
                }
            }

            if (o1.Has<BodyPart>() && o2.Has<BodyPart>()) {
                if (!o1.As<BodyPart>().Text.Equals(o2.As<BodyPart>().Text, StringComparison.CurrentCulture)) {
                    return false;
                }
            }
            return true;
        }

        public static bool SimilarTo(this ContentItem ci1, ContentItem ci2) {
            if (ci1.ContentType != ci2.ContentType)
                return false;

            if(ci1.Has<TitlePart>() && ci2.Has<TitlePart>()) {
                if (!ci1.As<TitlePart>().Title.Equals(ci2.As<TitlePart>().Title, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }
            if (ci1.Has<BodyPart>() && ci2.Has<BodyPart>())
            {
                if (!ci1.As<BodyPart>().Text.Equals(ci2.As<BodyPart>().Text, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}
