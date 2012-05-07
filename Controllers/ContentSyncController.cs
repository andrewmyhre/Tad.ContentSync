using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
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
            // get localcontent
            var localContent = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            // get remote content
            var remoteContent = _remoteSyncService.Fetch(new Uri(remote))
                .Where(rci => rci.Has<IdentityPart>())
                .ToList();

            _contentManager.Clear();



            var mappings = _remoteSyncService.GenerateSynchronisationMappings(localContent, remoteContent)
                .OrderByDescending(m=>m.EqualityRank);

            return View(mappings);
        }

        [HttpPost]
        public ActionResult Synchronise(string remote) {
            StringBuilder result = new StringBuilder();
            ImportContentSession importContentSession = new ImportContentSession(_contentManager);

            XDocument synchronisation = new XDocument();
            synchronisation.Add(new XElement("ContentSync"));
            var actions = Request.Form.ToPairs().Where(p => p.Key.StartsWith("action:"));
            foreach(var action in actions) {
                result.AppendLine(action.Key + ":" + action.Value);
                var actionDetail = action.Value.Split(':');
                var source = action.Key.Split(':'); // action:[identity]
                if (actionDetail.Length < 2) {
                    // todo: log
                    continue;
                }

                XElement sync = new XElement("Sync");
                var contentItem = importContentSession.Get(source[1]);
                if (contentItem==null) {
                    // todo: log
                    continue;
                }

                switch(actionDetail[0]) {
                    case "replace":
                        sync.Add(new XAttribute("Action", "Replace"));
                        sync.Add(new XAttribute("TargetId", actionDetail[1]));
                        sync.Add(_contentManager.Export(contentItem));
                        break;
                    case "push":
                        sync.Add(new XAttribute("Action", "Import"));
                        sync.Add(_contentManager.Export(contentItem));
                        break;
                }

                synchronisation.Element("ContentSync").Add(sync);
            }

            // send to other server
            string remoteImportEndpoint = remote + "/Admin/ContentImportExport/Import";
            HttpWebRequest post = HttpWebRequest.Create(remoteImportEndpoint) as HttpWebRequest;
            post.Method = "POST";
            using (var requestStream = post.GetRequestStream())
            using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8)){
                requestWriter.Write(synchronisation.ToString());
                requestWriter.Flush();
            }

            try {
                using (var response = post.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode == HttpStatusCode.Accepted) {
                        return RedirectToAction("Prepare", new {remote = remote});
                    } else {
                        return new ContentResult() {
                            Content = "server returned status " + response.StatusCode
                        };
                    }
                }
            } catch (Exception ex) {
                return new ContentResult()
                {
                    Content = ex.Message
                };
                
            }

        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}
