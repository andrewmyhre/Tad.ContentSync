using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Tad.ContentSync.Extensions;
using Tad.ContentSync.Models;
using Tad.ContentSync.Services;

namespace Tad.ContentSync.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly IShapeFactory _shapeFactory;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRemoteContentFetchService _remoteContentFetchService;
        private readonly ISynchronisationMapFactory _synchronisationMapFactory;
        private readonly IRepository<ContentSyncSettings> _contentSyncSettingsRepository;
        private readonly ISignals _signals;
        private readonly ICacheManager _cacheManager;
        public readonly ILogger Logger;

        public AdminController(IContentManager contentManager,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IContentDefinitionManager contentDefinitionManager,
            IRemoteContentFetchService remoteContentFetchService,
            ISynchronisationMapFactory synchronisationMapFactory,
            IRepository<ContentSyncSettings> contentSyncSettingsRepository,
            ISignals signals,
            ILoggerFactory loggerFactory,
            ICacheManager cacheManager) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _remoteContentFetchService = remoteContentFetchService;
            _synchronisationMapFactory = synchronisationMapFactory;
            _contentSyncSettingsRepository = contentSyncSettingsRepository;
            _signals = signals;
            _cacheManager = cacheManager;
            Logger = loggerFactory.CreateLogger(typeof (AdminController));
            }

        public ActionResult Index() {

            return View(Session["contentsync.remoteurl"] as string);
        }

        public ActionResult Prepare(string remote, string filter)
        {
            Logger.Debug("Diff " + filter);

            if (string.IsNullOrWhiteSpace(remote))
            {
                var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
                if (settings == null
                    || string.IsNullOrWhiteSpace(settings.RemoteUrl))
                {

                    _services.Notifier.Add(NotifyType.Warning,
                                           new LocalizedString(
                                               "You need to set a remote Orchard instance url"));
                    return RedirectToAction("Settings");

                }
                remote = settings.RemoteUrl;
            }


            // make sure we can reach the remote
            List<RemoteContentItem> remoteContent = null;
            try
            {
                remoteContent=_remoteContentFetchService.Fetch(new Uri(remote))
                    .Where(rci => rci.ContentItem.Has<IdentityPart>())
                    .ToList();
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var localContent = _contentManager
                .Query(VersionOptions.Published)
                .Join<IdentityPartRecord>()
                .List();


            _contentManager.Clear();

            var viewModel = new ContentSyncViewModel()
            {
                RemoteServerUrl = remote,
                Mappings =
                    _synchronisationMapFactory.BuildSynchronisationMap(localContent, remoteContent).
                    ToList(),
                GeneratedOn = DateTime.Now
            };

            switch(filter)
            {
                case "same":
                    viewModel.Mappings = viewModel.Mappings.Where(m => m.EqualityRank == 0);
                    break;
                case "different":
                    viewModel.Mappings = viewModel.Mappings.Where(m => m.EqualityRank == 1);
                    break;
                case "mismatch":
                    viewModel.Mappings = viewModel.Mappings.Where(m => m.EqualityRank == 2);
                    break;
                case "localonly":
                    viewModel.Mappings = viewModel.Mappings.Where(m => !m.Balanced && m.Local != null && (m.Similar == null || m.Similar.Count() == 0));
                    break;
                case "remoteonly":
                    viewModel.Mappings = viewModel.Mappings.Where(m => !m.Balanced && m.Remote != null);
                    break;
                default:
                    viewModel.Mappings = viewModel.Mappings.OrderByDescending(m => m.EqualityRank);
                    break;
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Synchronise(string remote, string filter) {

            if (string.IsNullOrWhiteSpace(remote))
            {
                var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
                if (settings == null
                    || string.IsNullOrWhiteSpace(settings.RemoteUrl))
                {

                    _services.Notifier.Add(NotifyType.Warning,
                                           new LocalizedString("You need to set a remote Orchard instance url"));
                    return RedirectToAction("Settings");

                }
                remote = settings.RemoteUrl;
            }

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

            Logger.Debug(synchronisation.ToString());

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
                        _services.Notifier.Add(NotifyType.Information, 
                            new LocalizedString("The content was published successfully"));
                        _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);
                    } else {
                        _services.Notifier.Add(NotifyType.Error,
                            new LocalizedString("A problem occurred: " + response.StatusDescription));
                    }
                }
            } catch (Exception ex) {
                _services.Notifier.Add(NotifyType.Error,
                    new LocalizedString("A problem occurred: " + ex.Message));
            }

            return RedirectToAction("Prepare", new { remote = remote, filter = filter });
        }

        public ActionResult Settings()
        {
            var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();

            return View(settings);
        }

        [HttpPost]
        public ActionResult UpdateSettings(string remoteUrl)
        {
            ContentSyncSettings settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
            if (settings == null)
            {
                settings = new ContentSyncSettings() {RemoteUrl = remoteUrl};
                _contentSyncSettingsRepository.Create(settings);
            } else
            {
                settings.RemoteUrl = remoteUrl;
                _contentSyncSettingsRepository.Update(settings);
            }
            _contentSyncSettingsRepository.Flush();
            
            _services.Notifier.Add(NotifyType.Information, new LocalizedString("Settings updated"));

            _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);

            return RedirectToAction("Settings");
        }

        public ActionResult Refresh(string filter)
        {
            _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);

            return RedirectToAction("Prepare", new {filter=filter});
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}
