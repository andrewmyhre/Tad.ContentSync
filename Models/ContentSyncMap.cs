using System.Collections.Generic;
using System.Linq;

namespace Tad.ContentSync.Models
{
    public class ContentSyncMap
    {
        public ContentItemSyncInfo Local { get; set; }
        public string Identifier { get; set; }
        public ContentItemSyncInfo Remote { get; set; }
        public bool Balanced { get { return Local != null && Remote != null; } }
        public bool Equal { get; set; }
        public string ContentType { get; set; }

        public IEnumerable<ContentItemSyncInfo> Similar { get; set; }

        public int EqualityRank { get {
            if (Balanced && Equal)
                return 0;
            else if (Balanced)
                return 1;
            else if (Similar != null && Similar.Count() > 0)
                return 2;

            return 3;
        }}

    }
}
