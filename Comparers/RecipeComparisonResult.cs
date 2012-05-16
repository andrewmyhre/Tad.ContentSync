using System.Collections.Generic;
using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class RecipeComparisonResult
    {
        public RecipeComparisonResult(IEnumerable<ContentPair> matching, IEnumerable<ContentPair> unmatched)
        {
            Matching = matching;
            Unmatched = unmatched;
        }

        public IEnumerable<ContentPair> Matching { get; private set; }
        public IEnumerable<ContentPair> Unmatched { get; set; }
    }
}