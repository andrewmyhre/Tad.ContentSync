using System.Xml.Linq;
using Orchard.ContentManagement;

namespace Tad.ContentSync.Models {
    public class ContentItemSyncInfo {
        public ContentItemSyncInfo() {
            
        }
        public ContentItemSyncInfo(ContentItem contentItem, object detailShape, object summaryShape, XElement itemXml) {
            ContentItem = contentItem;
            DetailShape = detailShape;
            SummaryShape = summaryShape;
            ItemXml = itemXml;
        }

        public ContentItem ContentItem { get; set; }
        public dynamic DetailShape { get; set; }
        public dynamic SummaryShape { get; set; }
        public XElement ItemXml { get; set; }
        public string Preview {
            get {
                if (ItemXml.Element("TitlePart") != null)
                    return ItemXml.Element("TitlePart").Attribute("Title").Value;
                if (ItemXml.Element("WidgetPart") != null)
                    return ItemXml.Element("WidgetPart").Attribute("Title").Value;
                if (ItemXml.Element("AutoroutePart") != null)
                    return ItemXml.Element("AutoroutePart").Attribute("Path").Value;
                if (ItemXml.Element("BodyPart") != null)
                    return ItemXml.Element("BodyPart").Attribute("Text").Value;
                return ItemXml.Name.LocalName;
            }
        }
    }
}