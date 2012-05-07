using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using ContentSync.Extensions;

namespace ContentSync.Models
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
