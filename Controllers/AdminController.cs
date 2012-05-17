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
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Tad.ContentSync.Comparers;
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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICacheManager _cacheManager;
        private readonly IImportExportService _importExportService;
        private readonly IRecipeParser _recipeParser;
        private readonly IRemoteImportService _remoteImportService;
        private readonly IEnumerable<IHardComparer> _hardComparers;
        private readonly IEnumerable<ISoftComparer> _softComparers;
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
            ICacheManager cacheManager,
            IImportExportService importExportService,
            IRecipeParser recipeParser,
            IRemoteImportService remoteImportService,
            IEnumerable<IHardComparer> hardComparers, 
            IEnumerable<ISoftComparer> softComparers) {
            _contentManager = contentManager;
            _services = services;
            _shapeFactory = shapeFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _remoteContentFetchService = remoteContentFetchService;
            _synchronisationMapFactory = synchronisationMapFactory;
            _contentSyncSettingsRepository = contentSyncSettingsRepository;
            _signals = signals;
            _loggerFactory = loggerFactory;
            _cacheManager = cacheManager;
            _importExportService = importExportService;
            _recipeParser = recipeParser;
            _remoteImportService = remoteImportService;
            _hardComparers = hardComparers;
            _softComparers = softComparers;
            Logger = loggerFactory.CreateLogger(typeof (AdminController));
            }



        public ActionResult Overview(string remote, string filter)
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
            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var localRecipe = LocalContentRecipe();

            var viewModel = new OverviewViewModel()
            {
                RemoteServerUrl = remote,
                LocalOnly = BuildLocalOnlyViewModel(localRecipe, remoteRecipe),
                Different = BuildDifferencesViewModel(localRecipe, remoteRecipe),
                Mismatches = BuildMismatchesViewModel(localRecipe, remoteRecipe),
                RemoteOnly = BuildRemoteOnlyViewModel(localRecipe, remoteRecipe),
                GeneratedOn = DateTime.Now
            };

            return View(viewModel);
        }

        public ActionResult LocalOnly(string remote)
        {
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

            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var viewModel = BuildLocalOnlyViewModel(LocalContentRecipe(), remoteRecipe);

            return View("LocalOnly", viewModel);
        }

        

        public ActionResult RemoteOnly(string remote)
        {
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

            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var viewModel = BuildRemoteOnlyViewModel(LocalContentRecipe(), remoteRecipe);

            return View("RemoteOnly", viewModel);
        }

        private IOrderedEnumerable<IGrouping<string, IGrouping<XElement, ContentPair>>> BuildDifferencesViewModel(Recipe localRecipe, Recipe remoteRecipe)
        {
            var matched = new RecipeComparer().Compare(localRecipe, remoteRecipe,
                                                       (left, right) => _hardComparers.All(comparer => comparer.IsMatch(left, right))
                                                       && _softComparers.Any(comparer => !comparer.IsMatch(left, right, true)));
            var differences = matched.Matching
                .OrderBy(pair => pair.Left.DisplayLabel())
                .OrderBy(pair => pair.Left.PartType())
                .GroupBy(pair => pair.Left)
                .GroupBy(layer => layer.Key.LayerName())
                .OrderBy(layer => layer.Key);
            return differences;
        }

        private IOrderedEnumerable<IGrouping<string, IGrouping<XElement, ContentPair>>> BuildMismatchesViewModel(Recipe localRecipe, Recipe remoteRecipe)
        {
            var contentTypeComparer = new ContentTypeContentComparer();
            var identityComparer = new IdentifierContentComparer();
            var layerComparer = new LayerNameComparer();

            var mismatched = new RecipeComparer()
                .Compare(localRecipe, remoteRecipe,
                (left, right) => 
                    contentTypeComparer.IsMatch(left, right) /*&& layerComparer.IsMatch(left,right)*/
                    && (!localRecipe.RecipeSteps.SingleOrDefault(step=>step.Name=="Data").Step.Elements()
                                    .Any(element=>element.Attribute("Id").Value.Equals(right.Attribute("Id").Value, StringComparison.InvariantCulture))
                        ||
                        !remoteRecipe.RecipeSteps.SingleOrDefault(step=>step.Name=="Data").Step.Elements()
                                    .Any(element=>element.Attribute("Id").Value.Equals(left.Attribute("Id").Value, StringComparison.InvariantCulture)))
                    && (_softComparers.Where(comparer => comparer.IsMatch(left, right)).Count() > 1))
                .Matching // all items of same type and title, body or layer name match
                .Where(pair => !identityComparer.IsMatch(pair.Left, pair.Right));
            // where the identifier is different

            var groupedMismatches = mismatched
                .GroupBy(pair => pair.Left)
                .GroupBy(set => set.Key.LayerName())
                .OrderBy(layer => layer.Key);
            return groupedMismatches;
        }

        private IOrderedEnumerable<IGrouping<string, IGrouping<XElement, ContentPair>>> BuildLocalOnlyViewModel(Recipe localRecipe, Recipe remoteRecipe)
        {
            var layerNameComparer = new LayerNameComparer();

            var comparison = new RecipeComparer()
                .Compare(localRecipe, remoteRecipe,
                            (left, right) => _hardComparers.All(comparer=>comparer.IsMatch(left, right)));

            var localonly = comparison.Unmatched
                .Where(pair => pair.Right == null)
                .OrderBy(pair => pair.Left.DisplayLabel())
                .OrderBy(pair => pair.Left.PartType())
                .GroupBy(pair => pair.Left)
                .GroupBy(layer => layer.Key.LayerName())
                .OrderBy(layer => layer.Key);
            return localonly;
        }
        private IOrderedEnumerable<IGrouping<string, IGrouping<XElement, ContentPair>>> BuildRemoteOnlyViewModel(Recipe localRecipe, Recipe remoteRecipe)
        {
            var contentTypeComparer = new ContentTypeContentComparer();
            var identifierComparer = new IdentifierContentComparer();
            var layerNameComparer = new LayerNameComparer();

            var comparison = new RecipeComparer().Compare(localRecipe, remoteRecipe,
                            (left, right) => contentTypeComparer.IsMatch(left, right)
                                            && (identifierComparer.IsMatch(left, right) ||
                                            layerNameComparer.IsMatch(left, right)));

            var localonly = comparison.Unmatched
                .Where(pair => pair.Left == null)
                .OrderBy(pair => pair.Right.DisplayLabel())
                .OrderBy(pair => pair.Right.PartType())
                .GroupBy(pair => pair.Right)
                .GroupBy(layer => layer.Key.LayerName())
                .OrderBy(layer => layer.Key);
            return localonly;
        }

        private Recipe LocalContentRecipe()
        {
            var recipeDocument = LocalRecipeDocument();
            var recipe = _recipeParser.ParseRecipe(recipeDocument.ToString());
            return recipe;
        }

        private XDocument LocalRecipeDocument()
        {
            var contentQuery = _contentManager.Query().ForVersion(VersionOptions.Published);

            var exportedItems = contentQuery.List().Select(item => item.ContentManager.Export(item));

            var recipeDocument = new XDocument(new XElement("Orchard", new XElement("Data", exportedItems)));
            return recipeDocument;
        }

        public enum ComparisonType
        {
            Overview,
            LocalOnly,
            RemoteOnly,
            Differences,
            Mismatches
        }

        public ActionResult Comparison(ComparisonType type)
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
            string remote = settings.RemoteUrl;

            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            ContentComparisonViewModel viewModel = new ContentComparisonViewModel();
            IOrderedEnumerable<IGrouping<string, IGrouping<XElement, ContentPair>>> comparison = null;
            switch(type)
            {
                case ComparisonType.LocalOnly:
                    comparison = BuildLocalOnlyViewModel(LocalContentRecipe(), remoteRecipe);
                    break;
                    case ComparisonType.RemoteOnly:
                    comparison = BuildRemoteOnlyViewModel(LocalContentRecipe(), remoteRecipe);
                    break;
                    case ComparisonType.Differences:
                        comparison = BuildDifferencesViewModel(LocalContentRecipe(), remoteRecipe);
                    break;
                    case ComparisonType.Mismatches:
                        comparison = BuildMismatchesViewModel(LocalContentRecipe(), remoteRecipe);
                    break;
            }
            viewModel.Comparison = comparison;
            viewModel.RemoteServerUrl = remote;

            return View(Enum.GetName(typeof(ComparisonType), type), viewModel);
        }

        public ActionResult Different()
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
            string remote = settings.RemoteUrl;

            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var differences = BuildDifferencesViewModel(LocalContentRecipe(), remoteRecipe);

            return View("Differences", differences);
        }

        

        public ActionResult Mismatches()
        {
            var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
            if (settings == null || string.IsNullOrWhiteSpace(settings.RemoteUrl))
            {
                _services.Notifier.Add(NotifyType.Warning,
                                        new LocalizedString(
                                            "You need to set a remote Orchard instance url"));
                return RedirectToAction("Settings");

            }
            string remote = settings.RemoteUrl;

            Recipe remoteRecipe = null;
            try
            {
                remoteRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString(ex.Message));
                return RedirectToAction("Settings");
            }

            // get localcontent
            var groupedMismatches = BuildMismatchesViewModel(LocalContentRecipe(), remoteRecipe);

            return View(groupedMismatches);
        }

        

        [HttpPost]
        public ActionResult Synchronise(string redirectAction, string direction, bool report=false) {

            var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
            if (settings == null
                || string.IsNullOrWhiteSpace(settings.RemoteUrl))
            {

                _services.Notifier.Add(NotifyType.Warning,
                                        new LocalizedString("You need to set a remote Orchard instance url"));
                return RedirectToAction("Settings");

            }
            string remote = settings.RemoteUrl;

            Recipe sourceRecipe = null;

            StringBuilder result = new StringBuilder();
            ISynchronisationJobBuilder synchronisationJobBuilder = null;
            ISynchronisationJobRunner syncJobRunner = null;
            if (direction == "up")
            {
                sourceRecipe = LocalContentRecipe();
                syncJobRunner = new PushSynchronisationJobRunner(remote, _services, _signals);
            }
            else
            {
                try
                {
                    sourceRecipe = _remoteContentFetchService.FetchRecipe(new Uri(remote));
                }
                catch (Exception ex)
                {
                    _services.Notifier.Add(NotifyType.Error,
                                           new LocalizedString(ex.Message));
                    return RedirectToAction("Settings");
                }
                syncJobRunner = new PullSynchronisationJobRunner(_remoteImportService);
            }
            synchronisationJobBuilder = new SynchronisationJobBuilder(sourceRecipe, _loggerFactory);

            var actions = Request.Form["sync"].Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            foreach(var action in actions) {
                result.AppendLine("sync:" + action);
                string localIdentifier = "", remoteIdentifier = "";
                if (action.IndexOf("::") > 0) { localIdentifier=action.Substring(0, action.IndexOf("::")); }
                if (action.IndexOf("::") < action.Length-1) {remoteIdentifier=action.Substring(action.IndexOf("::") + 2);}

                if (direction == "up")
                {
                    synchronisationJobBuilder.AddStep(localIdentifier, remoteIdentifier);
                }
                else if (direction == "down")
                {
                    synchronisationJobBuilder.AddStep(remoteIdentifier, localIdentifier);
                }
            }

            if (report)
            {
                return new ContentResult()
                           {
                               Content =
                                   new XElement("ContentSync", synchronisationJobBuilder.SynchronisationSteps).ToString(),
                               ContentType = "text/xml"
                           };
            }
            syncJobRunner.Process(synchronisationJobBuilder);

            return RedirectToAction(redirectAction??"Overview");
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

        [HttpPost]
        public ActionResult CorrectMismatches(string direction)
        {
            var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
            if (settings == null
                || string.IsNullOrWhiteSpace(settings.RemoteUrl))
            {

                _services.Notifier.Add(NotifyType.Warning,
                                        new LocalizedString("You need to set a remote Orchard instance url"));
                return RedirectToAction("Settings");

            }
            string remote = settings.RemoteUrl;

            StringBuilder result = new StringBuilder();
            ImportContentSession importContentSession = new ImportContentSession(_contentManager);

            XDocument synchronisation = new XDocument();
            synchronisation.Add(new XElement("ContentSync"));
            var actions = Request.Form.ToPairs().Where(p => p.Key == "sync");
            foreach (var action in actions)
            {
                //result.AppendLine(action.Key + ":" + action.Value);
                var localIdentifier = action.Value.Substring(0, action.Value.IndexOf("::"));
                var remoteIdentifier = action.Value.Substring(action.Value.IndexOf("::")+2);

                XElement sync = new XElement("Sync");
                var contentItem = importContentSession.Get(localIdentifier);
                if (contentItem == null)
                {
                    // todo: log
                    continue;
                }

                sync.Add(new XAttribute("Action", "Replace"));
                sync.Add(new XAttribute("TargetId", remoteIdentifier));
                sync.Add(_contentManager.Export(contentItem));

                synchronisation.Element("ContentSync").Add(sync);
            }

            Logger.Debug(synchronisation.ToString());

            // send to other server
            string remoteImportEndpoint = remote + "/Admin/ContentImportExport/Import";
            HttpWebRequest post = HttpWebRequest.Create(remoteImportEndpoint) as HttpWebRequest;
            post.Method = "POST";
            using (var requestStream = post.GetRequestStream())
            using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8))
            {
                requestWriter.Write(synchronisation.ToString());
                requestWriter.Flush();
            }

            try
            {
                using (var response = post.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _services.Notifier.Add(NotifyType.Information,
                            new LocalizedString("The content was published successfully"));
                        _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);
                    }
                    else
                    {
                        _services.Notifier.Add(NotifyType.Error,
                            new LocalizedString("A problem occurred: " + response.StatusDescription));
                    }
                }
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                    new LocalizedString("A problem occurred: " + ex.Message));
            }

            return RedirectToAction("Mismatches");
        }

        [HttpPost]
        public ActionResult Export(string[] sync, string direction)
        {
            if (sync == null)
            {
                return RedirectToAction("Different");
            }

            var settings = _contentSyncSettingsRepository.Table.SingleOrDefault();
            if (settings == null
                || string.IsNullOrWhiteSpace(settings.RemoteUrl))
            {

                _services.Notifier.Add(NotifyType.Warning,
                                        new LocalizedString("You need to set a remote Orchard instance url"));
                return RedirectToAction("Settings");

            }
            string remote = settings.RemoteUrl;

            StringBuilder result = new StringBuilder();
            ImportContentSession importContentSession = new ImportContentSession(_contentManager);

            XDocument synchronisation = new XDocument();
            synchronisation.Add(new XElement("ContentSync"));
            foreach (var action in sync)
            {
                XElement syncXml = new XElement("Sync");
                var contentItem = importContentSession.Get(action);
                if (contentItem == null)
                {
                    // todo: log
                    continue;
                }

                syncXml.Add(new XAttribute("Action", "Import"));
                syncXml.Add(_contentManager.Export(contentItem));

                synchronisation.Element("ContentSync").Add(syncXml);
            }

            Logger.Debug(synchronisation.ToString());

            // send to other server
            string remoteImportEndpoint = remote + "/Admin/ContentImportExport/Import";
            HttpWebRequest post = HttpWebRequest.Create(remoteImportEndpoint) as HttpWebRequest;
            post.Method = "POST";
            using (var requestStream = post.GetRequestStream())
            using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8))
            {
                requestWriter.Write(synchronisation.ToString());
                requestWriter.Flush();
            }

            try
            {
                using (var response = post.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _services.Notifier.Add(NotifyType.Information,
                            new LocalizedString("The content was published successfully"));
                        _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);
                    }
                    else
                    {
                        _services.Notifier.Add(NotifyType.Error,
                            new LocalizedString("A problem occurred: " + response.StatusDescription));
                    }
                }
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                    new LocalizedString("A problem occurred: " + ex.Message));
            }

            return RedirectToAction("Different");
        }

        public ActionResult Refresh(string filter)
        {
            _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);

            return RedirectToAction("Overview", new {filter=filter});
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable)
        {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Settings.GetModel<ContentTypeSettings>().Creatable && (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}
