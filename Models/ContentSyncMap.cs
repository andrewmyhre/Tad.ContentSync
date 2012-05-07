using System.Collections.Generic;

namespace Tad.ContentSync.Models
{
    public class ContentSyncMap
    {
        public ContentItemSyncInfo Local { get; set; }
        public string Identifier { get; set; }
        public ContentItemSyncInfo Remote { get; set; }
        public bool Balanced { get { return Local != null && Remote != null; } }
        public bool Equal { get; set; }

        public IEnumerable<ContentItemSyncInfo> Similar { get; set; }

        public int EqualityRank { get {
            if (Balanced && Equal)
                return 0;
            if (Balanced)
                return 1;
            return 2;
        }}

    }
}
