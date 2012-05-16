using System;
using System.Xml.Linq;
using Tad.ContentSync.Extensions;

namespace Tad.ContentSync.Comparers
{
    public class LayerNameComparer : ISoftComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives = false)
        {
            var leftLayer = leftContentItem.LayerName();
            var rightLayer = rightContentItem.LayerName();
            if (!allowFalsePositives)
            {
                if (string.IsNullOrWhiteSpace(leftLayer)
                    || string.IsNullOrWhiteSpace(rightLayer))
                    return false;
            }

            return leftContentItem.LayerName().Equals(rightContentItem.LayerName(),
                                                      StringComparison.InvariantCultureIgnoreCase);
        }
    }
}