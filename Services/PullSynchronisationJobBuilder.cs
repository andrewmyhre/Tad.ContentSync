using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public class PullSynchronisationJobBuilder : ISynchronisationJobBuilder
    {
        private readonly ImportContentSession _importContentSession;
        private readonly IContentManager _contentManager;
        private readonly Recipe _remoteRecipe;
        private readonly IRemoteImportService _importService;
        private List<XElement> _imports = new List<XElement>();
        private RecipeStep dataStep;

        public PullSynchronisationJobBuilder(
            ImportContentSession importContentSession, 
            IContentManager contentManager, 
            Recipe remoteRecipe,
            IRemoteImportService importService)
        {
            _importContentSession = importContentSession;
            _contentManager = contentManager;
            _remoteRecipe = remoteRecipe;
            _importService = importService;
            dataStep = _remoteRecipe.RecipeSteps.SingleOrDefault(s => s.Name == "Data");
        }

        public void Process(string remoteInstanceUrl, bool rollback)
        {
            _importService.Import(_imports.Select(ImportSyncAction.Parse), rollback);
        }

        public void AddStep(string localIdentifier, string remoteIdentifier)
        {
            XElement sync = new XElement("Sync");
            var contentElement = dataStep.Step.Elements().SingleOrDefault(e => e.Attribute("Id").Value.Equals(remoteIdentifier, StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrWhiteSpace(localIdentifier)
                || string.IsNullOrWhiteSpace(remoteIdentifier)
                || localIdentifier.Equals(remoteIdentifier))
            {
                sync.Add(new XAttribute("Action", "Import"));
                sync.Add(contentElement);
            }
            else if (!localIdentifier.Equals(remoteIdentifier))
            {
                sync.Add(new XAttribute("Action", "Replace"));
                sync.Add(new XAttribute("TargetId", localIdentifier));
                sync.Add(contentElement);
            } else
            {
                sync.Add(new XAttribute("Action", "Import"));
                sync.Add(contentElement);
            }
            _imports.Add(sync);
        }
    }
}