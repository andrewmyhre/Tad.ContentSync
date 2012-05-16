using System;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class AutorouteContentComparer : IRecipeStepComparer, ISoftComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives=false)
        {
            var leftRoute = leftContentItem.Element("AutoroutePart");
            var rightRoute = rightContentItem.Element("AutoroutePart");

            if (leftRoute == null || rightRoute == null)
            {
                return allowFalsePositives;
            }

            if (leftRoute.Attribute("Alias") == null && rightRoute.Attribute("Alias") == null)
                return allowFalsePositives;

            if (leftRoute.Attribute("Alias") == null || rightRoute.Attribute("Alias") == null)
                return false;

            return leftRoute.Attribute("Alias").Value
                .Equals(rightRoute.Attribute("Alias").Value, StringComparison.InvariantCultureIgnoreCase);

        }
    }
}