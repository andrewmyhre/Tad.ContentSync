using Orchard.ContentManagement;

namespace ContentSync.Models {
    public class ContentItemSyncInfo {
        public ContentItemSyncInfo() {
            
        }
        public ContentItemSyncInfo(ContentItem contentItem, object shape) {
            ContentItem = contentItem;
            Shape = shape;
        }

        public ContentItem ContentItem { get; set; }
        public dynamic Shape { get; set; }
    }
}