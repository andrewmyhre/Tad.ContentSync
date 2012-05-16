using System;
using System.Linq;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class IdentifierContentComparer : IHardComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives = false)
        {
            var identityLeft = leftContentItem.Attribute("Id");
            var identityRight = rightContentItem.Attribute("Id");

            if (identityLeft == null && identityRight == null)
                return allowFalsePositives;

            if (identityLeft == null || identityRight == null)
                return false;

            return identityLeft.Value
                .Equals(identityRight.Value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}