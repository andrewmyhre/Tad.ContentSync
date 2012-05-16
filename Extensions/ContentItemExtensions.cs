using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Tad.ContentSync.Services;

namespace Tad.ContentSync.Extensions {
    public static class ContentItemExtensions {

        public static bool SharesIdentifierWith(this ContentItem item1, ContentItem item2)
        {
            if (item1.Has<IdentityPart>() && item2.Has<IdentityPart>())
            {
                return item1.As<IdentityPart>().Identifier.Equals(item2.As<IdentityPart>().Identifier,
                                                               StringComparison.InvariantCultureIgnoreCase);
            }
            return false;
        }

        private static bool AreEqual(ContentItem item1, ContentItem item2, XElement item1Export, XElement item2Export)
        {
            //todo: this is a little too generous
            if (!item1.SharesIdentifierWith(item2))
                return false;

            if (item1.Has<TitlePart>() && item2.Has<TitlePart>())
            {
                if (!item1.As<TitlePart>().Title.Equals(item2.As<TitlePart>().Title, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            if (item1.Has<BodyPart>() && item2.Has<BodyPart>())
            {
                var text1 = item1.As<BodyPart>().Text;
                var text2 = item2.As<BodyPart>().Text;

                if (text1 == null || text2 == null)
                    return false;

                if (!item1.As<BodyPart>().Text.Equals(item2.As<BodyPart>().Text, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            // compare xml elements
            return Differences(item1Export, item2Export) == 0;
        }

        public static bool IsEqualTo(this ContentItem o1, ContentItem o2)
        {
            return AreEqual(o1, o2, o1.ContentManager.Export(o1), o2.ContentManager.Export(o2));
        }

        public static bool IsEqualTo(this ContentItem o1, RemoteContentItem remoteItem)
        {
            return AreEqual(o1, remoteItem.ContentItem, o1.ContentManager.Export(o1), remoteItem.Xml);
        }

        private static int Differences(XElement export1, XElement export2)
        {
            int differences = 0;
            var attributesToCompare = new string[] { "Title", "Text", "Name" };
            var elementsToCompare = new string[] { "BodyPart", "WidgetPart", "TitlePart", "LayerPart" };

            foreach (var element in export1.Elements())
            {
                if (!elementsToCompare.Contains(element.Name.LocalName))
                    continue;

                var counterpart = export2.Element(element.Name.LocalName);
                if (counterpart == null)
                    return 0;

                foreach (var attributeName in attributesToCompare)
                {
                    if (element.Attribute(attributeName) != null &&
                        counterpart.Attribute(attributeName) != null)
                    {
                        if (!element.Attribute(attributeName).Value.Equals(counterpart.Attribute(attributeName).Value, StringComparison.InvariantCultureIgnoreCase))
                            differences++;
                    }
                }
            }
            return differences;
        }

        private static int Similarity(XElement export1, XElement export2)
        {
            int similarity = 0;
            var attributesToCompare = new string[] {"Title", "Text", "Name"};
            var elementsToCompare = new string[] {"BodyPart", "WidgetPart", "TitlePart", "LayerPart"};

            foreach (var element in export1.Elements())
            {
                if (!elementsToCompare.Contains(element.Name.LocalName))
                    continue;

                var counterpart = export2.Element(element.Name.LocalName);
                if (counterpart == null)
                    return 0;

                foreach (var attributeName in attributesToCompare)
                {
                    if (element.Attribute(attributeName) != null &&
                        counterpart.Attribute(attributeName) != null)
                    {
                        if (element.Attribute(attributeName).Value.Equals(counterpart.Attribute(attributeName).Value, StringComparison.InvariantCultureIgnoreCase))
                            similarity++;
                    }
                }
            }
            return similarity;
        }

        public static bool Similarto(this ContentItem ci1, RemoteContentItem remoteItem)
        {
            return ci1.SimilarTo(remoteItem.ContentItem);
        }

        public static bool SimilarTo(this ContentItem ci1, ContentItem ci2) {
            var export1 = ci1.ContentManager.Export(ci1);
            var export2 = ci2.ContentManager.Export(ci2);

            return Similarity(export1,export2) > 1;
        }

        public static string PartValue(this XElement element, string partName, string partAttribute)
        {
            if (element.Element(partName) != null)
                if (element.Element(partName).Attribute(partAttribute) != null)
                    return element.Element(partName).Attribute(partAttribute).Value;
            return "";
        }

        public static string LayerName(this XElement element)
        {
            if (element.Element("CommonPart") != null && element.Element("CommonPart").Attribute("Container") != null)
            {
                string container = element.Element("CommonPart").Attribute("Container").Value;

                var layerNameIndex = container.IndexOf("Layer.LayerName=");
                if (layerNameIndex > -1)
                {
                    return container.Substring(layerNameIndex + 16);
                }
                
            }
            return element.Name.LocalName;
        }

        public static string ContentIdentifier(this XElement element)
        {
            return element.Attribute("Id").Value;
        }

        public static string DisplayLabel(this XElement element)
        {
            if (element.Element("TitlePart") != null && element.Element("TitlePart").Attribute("Title") != null)
            {
                return element.Element("TitlePart").Attribute("Title").Value;
            }
            if (element.Element("WidgetPart") != null && element.Element("WidgetPart").Attribute("Title") != null)
            {
                return element.Element("WidgetPart").Attribute("Title").Value;
            }
            if (element.Element("LayerPart") != null && element.Element("LayerPart").Attribute("Name") != null)
            {
                return element.Element("LayerPart").Attribute("Name").Value;
            }

            return "";
        }

        public static string PartType(this XElement element)
        {
            return element.Name.LocalName;
        }

        public static string Entitize(this XElement element)
        {
            foreach(var e in element.Elements())
            {
                e.SetValue("");
            }

            return element.ToString();
        }
    }
}