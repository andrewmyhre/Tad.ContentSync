using System;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class TitleContentComparer : IRecipeStepComparer, ISoftComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives=false)
        {
            var title1 = leftContentItem.Element("TitlePart");
            var title2 = rightContentItem.Element("TitlePart");

            // could be a widgetpart which has its own Title attribute
            if (title1 == null && title2 == null)
            {
                title1 = leftContentItem.Element("WidgetPart");
                title2 = rightContentItem.Element("WidgetPart");
            }

            if (title1 == null && title2 == null)
            {
                return allowFalsePositives;
            }

            if (title1 == null || title2 == null)
            {
                return false;
            }

            return title1.Attribute("Title").Value.Equals(title2.Attribute("Title").Value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}