using System;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public interface IHardComparer : IRecipeStepComparer
    {
        
    }

    public class ContentTypeContentComparer : IHardComparer
    {
        public bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives = false)
        {
            return leftContentItem.Name.LocalName.Equals(rightContentItem.Name.LocalName,
                                              StringComparison.InvariantCultureIgnoreCase);
        }
    }
}