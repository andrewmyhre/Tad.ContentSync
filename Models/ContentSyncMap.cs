using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement;

namespace ContentSync.Models
{
    public class ContentSyncMap
    {
        public ContentItemSyncInfo Local { get; set; }
        public string Identifier { get; set; }
        public ContentItemSyncInfo Remote { get; set; }
    }

    public class ContentItemSyncInfo {
        public ContentItem ContentItem { get; set; }
        public dynamic Shape { get; set; }
    }
}
