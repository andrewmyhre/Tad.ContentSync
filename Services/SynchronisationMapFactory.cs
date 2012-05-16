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

        public IEnumerable<ContentSyncMap> BuildSynchronisationMap(IEnumerable<ContentItem> localContent, List<RemoteContentItem> remoteContents)
        {
                        //ctx.Monitor(_signals.When(MapInvalidationTrigger));

                        _orchardServices.ContentManager.Clear();
                        List<RemoteContentItem> remoteContent = new List<RemoteContentItem>(remoteContents);
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

                            ContentSyncMap map = new ContentSyncMap(){ContentType=localItem.ContentType};
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
                                if (localItem.SharesIdentifierWith(remoteItem.ContentItem))
                                {
                                    var detailShape = _orchardServices.ContentManager.BuildDisplay(remoteItem.ContentItem, "Detail");
                                    var summaryShape = _orchardServices.ContentManager.BuildDisplay(remoteItem.ContentItem, "Summary");
                                    map.Remote = new ContentItemSyncInfo(remoteItem.ContentItem, detailShape, summaryShape, remoteItem.Xml);
                                    remoteContent.Remove(remoteItem);
                                    map.Equal = localItem.IsEqualTo(remoteItem);

                                    break;
                                }
                            }

                            if (map.Remote == null)
                            {
                                map.Similar = remoteContent.Where(r =>
                                    new ContentItemComparer(map.Local.ContentItem, _orchardServices.ContentManager).SimilarityTo(r.ContentItem) != 0)
                                    .Select(r =>
                                                {
                                                    dynamic detailShape = _orchardServices.ContentManager.BuildDisplay(
                                                        r.ContentItem, "Detail")
                                                        .Identifier(
                                                            _orchardServices.ContentManager.GetItemMetadata(r.ContentItem).Identity.
                                                                ToString());
                                                    dynamic summaryShape = _orchardServices.ContentManager.BuildDisplay(
                                                        r.ContentItem, "Summary")
                                                        .Identifier(
                                                            _orchardServices.ContentManager.GetItemMetadata(r.ContentItem).Identity.
                                                                ToString());
                                                    return new ContentItemSyncInfo(r.ContentItem, detailShape, summaryShape,r.Xml);
                                                }).ToList();
                            }

                            mappings.Add(map);
                        }

                        foreach (var remoteContentItem in remoteContent)
                        {
                            mappings.Add(new ContentSyncMap()
                                             {
                                                 ContentType=remoteContentItem.ContentItem.ContentType,
                                                 Remote = new ContentItemSyncInfo(remoteContentItem.ContentItem,
                                                                                  _orchardServices.ContentManager.
                                                                                      BuildDisplay(remoteContentItem.ContentItem,
                                                                                                   "Detail"),
                                                                                  _orchardServices.ContentManager.
                                                                                      BuildDisplay(remoteContentItem.ContentItem,
                                                                                                   "Summary"),
                                                                                  remoteContentItem.Xml),
                                                 Identifier = remoteContentItem.ContentItem.As<IdentityPart>().Identifier
                                             });
                        }

                        return mappings;
        }
    }
}