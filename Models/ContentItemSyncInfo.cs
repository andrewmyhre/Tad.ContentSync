using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

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
        public string Identifier { get { return ContentItem.As<IdentityPart>().Identifier; } }
        public string PreviewUrl {get
        {
            if (ItemXml.Element("AutoroutePart") != null && ItemXml.Element("AutoroutePart").Attribute("Alias") != null)
                return "/"+ItemXml.Element("AutoroutePart").Attribute("Alias").Value;

            return "/en/Admin/ContentSync/Preview/" + Identifier;
        }}

        private string _raw=null;
        public string ForDiff()
        {
            if (_raw == null)
            {


                StringBuilder xml = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings()
                                                 {
                                                     Indent = true,
                                                     NewLineHandling = NewLineHandling.Entitize,
                                                     NewLineOnAttributes = true
                                                 };

                using (XmlWriter w = XmlWriter.Create(xml, settings))
                {
                    // convert <part /> to <part></part> so the xml is represented correctly in html
                    foreach(var element in ItemXml.Elements()) {element.Value = "";}

                    w.WriteRaw(ItemXml.ToString(SaveOptions.None));
                    w.Flush();
                }
                _raw = xml.ToString();
            }
            return _raw;
        }
        public string Preview {
            get {
                if (ItemXml.Element("TitlePart") != null && ItemXml.Element("TitlePart").Attribute("Title") != null)
                    return ItemXml.Element("TitlePart").Attribute("Title").Value;
                if (ItemXml.Element("WidgetPart") != null && ItemXml.Element("WidgetPart").Attribute("Title") != null)
                    return ItemXml.Element("WidgetPart").Attribute("Title").Value;
                if (ItemXml.Element("AutoroutePart") != null && ItemXml.Element("AutoroutePart").Attribute("Path") != null)
                    return ItemXml.Element("AutoroutePart").Attribute("Path").Value;
                if (ItemXml.Element("BodyPart") != null && ItemXml.Element("BodyPart").Attribute("Text") != null)
                    return ItemXml.Element("BodyPart").Attribute("Text").Value;
                return ItemXml.Name.LocalName;
            }
        }
    }
}