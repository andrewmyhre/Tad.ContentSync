using System.Xml.Linq;
using Orchard;

namespace Tad.ContentSync.Comparers
{
    public interface IRecipeStepComparer : IDependency
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leftContentItem"></param>
        /// <param name="rightContentItem"></param>
        /// <param name="allowFalsePositives">When a content item doesn't have the relevant part this value is returned</param>
        /// <returns></returns>
        bool IsMatch(XElement leftContentItem, XElement rightContentItem, bool allowFalsePositives=false);
    }
}