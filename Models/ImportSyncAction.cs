using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace ContentSync.Models
{
    public class ImportSyncAction
    {
        public string Action { get; set; }
        public string TargetId { get; set; }
        public RecipeStep Step { get; set; }

        public static ImportSyncAction Parse(XElement sync) {
            var step = ((XElement) sync.FirstNode);
            return new ImportSyncAction() {
                Action = sync.Attribute("Action").Value,
                TargetId = sync.Attribute("TargetId") != null ? sync.Attribute("TargetId").Value : null,
                Step = new RecipeStep() {Name = step.Name.LocalName, Step = step}
            };
        }
    }
}
