using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public bool Equal { get {
            if (Balanced) {
                return Local.ContentItem.IsEqualTo(Remote.ContentItem);
            }
            return false;
        } }

        public IEnumerable<ContentItemSyncInfo> Similar { get; set; }
    }
}
