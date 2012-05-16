using System;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class BodyContentComparer : IRecipeStepComparer, ISoftComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives = false)
        {
            var body1 = leftContentItem.Element("BodyPart");
            var body2 = rightContentItem.Element("BodyPart");

            if (body1 == null && body2 == null)
                return allowFalsePositives;

            if (body1 == null || body2 == null)
                return false;

            if (body1.Attribute("Text") == null && body2.Attribute("Text") == null)
                return allowFalsePositives;

            if (body1.Attribute("Text") == null || body2.Attribute("Text") == null)
                return false;

            return body1.Attribute("Text").Value
                .Equals(body2.Attribute("Text").Value,StringComparison.InvariantCulture);
        }
    }
}