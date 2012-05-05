using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

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
                if (Local.ContentItem.Has<CommonPart>() && Remote.ContentItem.Has<CommonPart>()) {
                    var localPublished = Local.ContentItem.As<CommonPart>().PublishedUtc;
                    var remotePublished = Remote.ContentItem.As<CommonPart>().PublishedUtc;

                    if (localPublished.HasValue && remotePublished.HasValue) {
                        if (localPublished.Value <= remotePublished.Value) {
                            return true;
                        }
                    }
                }
            }
            return false;
        } }

        public IEnumerable<ContentItemSyncInfo> Similar { get; set; }
    }
}
