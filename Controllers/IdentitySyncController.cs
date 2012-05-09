using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Tad.ContentSync.Services;

namespace Tad.ContentSync.Controllers
{
    public class IdentitySyncController : Controller
    {
        private readonly IRemoteContentFetchService _remoteContentFetchService;
        private readonly ISynchronisationMapFactory _synchronisationMapFactory;
        private readonly IContentManager _contentManager;

        public IdentitySyncController(
            IRemoteContentFetchService remoteContentFetchService, 
            ISynchronisationMapFactory synchronisationMapFactory,
            IContentManager contentManager)
        {
            _remoteContentFetchService = remoteContentFetchService;
            _synchronisationMapFactory = synchronisationMapFactory;
            _contentManager = contentManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        
        public ActionResult Diff(string remote)
        {
            // get remote content
            var remoteContent = _remoteContentFetchService.Fetch(new Uri(remote))
                .Where(rci => rci.Has<IdentityPart>())
                .ToList();

            // get localcontent
            var localContent = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            var mappings = _synchronisationMapFactory.BuildSynchronisationMap(localContent, remoteContent)
                .OrderByDescending(m => m.EqualityRank);

            return View(mappings);
        }
    }
}
