using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Tad.ContentSync.Extensions;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public class SynchronisationMapFactory : ISynchronisationMapFactory
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IShapeFactory _shapeFactory;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        public static string MapInvalidationTrigger { get { return "ContentSync.SynchronisationMapInvalid"; } }
        public static string MapCacheKey { get { return "ContentSync.SynchronisationMapInvalid"; } }

        public SynchronisationMapFactory(IOrchardServices orchardServices, IShapeFactory shapeFactory, 
            ICacheManager cacheManager, ISignals signals)
        {
            _orchardServices = orchardServices;
            _shapeFactory = shapeFactory;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public IEnumerable<ContentSyncMap> BuildSynchronisationMap(IEnumerable<ContentItem> localContent, IEnumerable<ContentItem> remoteContents)
        {
            return _cacheManager.Get<string, IEnumerable<ContentSyncMap>>(MapCacheKey,
                ctx =>
                    {
                        ctx.Monitor(_signals.When(MapInvalidationTrigger));

                        _orchardServices.ContentManager.Clear();
                        List<ContentItem> remoteContent = new List<ContentItem>(remoteContents);
                        dynamic Shape = _shapeFactory;

                        //var list = DetailShape.List();
                        //list.AddRange(localContent.Select(ci => _orchardServices.ContentManager.BuildDisplay(ci, "Summary")));

                        List<ContentSyncMap> mappings = new List<ContentSyncMap>();

                        foreach (var localItem in localContent)
                        {
                            if (!localItem.Has<IdentityPart>())
                                continue;

                            if (!ContentSync.SyncableContentTypes.Contains(localItem.ContentType))
                                continue;

                            ContentSyncMap map = new ContentSyncMap();
                            map.Local = new ContentItemSyncInfo(localItem,
                                                                _orchardServices.ContentManager.BuildDisplay(localItem,
                                                                                                             "Detail"),
                                                                _orchardServices.ContentManager.BuildDisplay(localItem,
                                                                                                             "Summary"),
                                                                _orchardServices.ContentManager.Export(localItem));
                            map.Identifier =
                                _orchardServices.ContentManager.GetItemMetadata(localItem).Identity.ToString();

                            // try to find a match
                            for (int i = 0; i < remoteContent.Count; i++)
                            {
                                var remoteItem = remoteContent[i];
                                var localIdentifier =
                                    _orchardServices.ContentManager.GetItemMetadata(localItem).Identity.ToString();
                                var remoteIdentifier =
                                    _orchardServices.ContentManager.GetItemMetadata(remoteItem).Identity.ToString();
                                if (localIdentifier.Equals(remoteIdentifier))
                                {
                                    var detailShape = _orchardServices.ContentManager.BuildDisplay(remoteItem, "Detail");
                                    var summaryShape = _orchardServices.ContentManager.BuildDisplay(remoteItem,
                                                                                                    "Summary");
                                    map.Remote = new ContentItemSyncInfo(remoteItem, detailShape, summaryShape,
                                                                         _orchardServices.ContentManager.Export(
                                                                             remoteItem));
                                    remoteContent.Remove(remoteItem);
                                    map.Equal = localItem.IsEqualTo(remoteItem, _orchardServices.ContentManager);

                                    break;
                                }
                            }

                            if (map.Remote == null)
                            {
                                map.Similar = remoteContent.Where(r => map.Local.ContentItem.SimilarTo(r))
                                    .Select(r =>
                                                {
                                                    dynamic detailShape = _orchardServices.ContentManager.BuildDisplay(
                                                        r, "Detail")
                                                        .Identifier(
                                                            _orchardServices.ContentManager.GetItemMetadata(r).Identity.
                                                                ToString());
                                                    dynamic summaryShape = _orchardServices.ContentManager.BuildDisplay(
                                                        r, "Summary")
                                                        .Identifier(
                                                            _orchardServices.ContentManager.GetItemMetadata(r).Identity.
                                                                ToString());
                                                    return new ContentItemSyncInfo(r, detailShape, summaryShape,
                                                                                   _orchardServices.ContentManager.
                                                                                       Export(r));
                                                }).ToList();
                            }

                            mappings.Add(map);
                        }

                        foreach (var remoteContentItem in remoteContent)
                        {
                            mappings.Add(new ContentSyncMap()
                                             {
                                                 Remote = new ContentItemSyncInfo(remoteContentItem,
                                                                                  _orchardServices.ContentManager.
                                                                                      BuildDisplay(remoteContentItem,
                                                                                                   "Detail"),
                                                                                  _orchardServices.ContentManager.
                                                                                      BuildDisplay(remoteContentItem,
                                                                                                   "Summary"),
                                                                                  _orchardServices.ContentManager.Export
                                                                                      (remoteContentItem)),
                                                 Identifier = remoteContentItem.As<IdentityPart>().Identifier
                                             });
                        }

                        return mappings;
                    });
        }
    }
}