using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;

namespace Tad.ContentSync.Services
{
    public class SynchronisationJobBuilder : ISynchronisationJobBuilder
    {
        private readonly Recipe _sourceRecipe;
        private readonly List<XElement> _synchronisationSteps = new List<XElement>();
        private RecipeStep dataStep;
        public ILogger Logger { get; set; }

        public List<XElement> SynchronisationSteps { get { return _synchronisationSteps; } } 

        public SynchronisationJobBuilder(Recipe sourceRecipe, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(typeof(SynchronisationJobBuilder));
            _sourceRecipe = sourceRecipe;
            dataStep = _sourceRecipe.RecipeSteps.SingleOrDefault(s => s.Name == "Data");
        }

        //todo: unpick the local/remote distinction, concept should be SOURCE -> TARGET where target is optional (if importing a new item)
        public void AddStep(string sourceItemIdentifier, string targetItemIdentifier=null)
        {
            var contentElement = dataStep.Step.Elements().SingleOrDefault(e => e.Attribute("Id").Value.Equals(sourceItemIdentifier, StringComparison.InvariantCultureIgnoreCase));
            XElement sync = new XElement("Sync");
            if (!sourceItemIdentifier.Equals(targetItemIdentifier) && !string.IsNullOrWhiteSpace(targetItemIdentifier))
            {
                sync.Add(new XAttribute("Action", "Replace"));
                sync.Add(new XAttribute("TargetId", targetItemIdentifier));
                sync.Add(contentElement);
            }
            else
            {
                sync.Add(new XAttribute("Action", "Import"));
                sync.Add(contentElement);
            }

            // get the container too
            var commonPart = contentElement.Element("CommonPart");
            if (commonPart != null)
            {
                var containerIdentifier = commonPart.Attribute("Container");
                if (containerIdentifier != null)
                {
                    var containerElement = dataStep.Step.Elements().SingleOrDefault(e => e.Attribute("Id").Value.Equals(containerIdentifier.Value));

                    if (containerElement == null)
                    {
                        Logger.Warning("Could not find container {0} for content item {1}", containerIdentifier, sourceItemIdentifier);
                        return;
                    }
                    _synchronisationSteps.Add(new XElement("Sync", new XAttribute("Action", "Import"), containerElement));
                }
            }

            _synchronisationSteps.Add(sync);
        }
    }
}