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
            var attributesToCompare = new string[] { "Title", "Text" };
            var elementsToCompare = new string[] { "BodyPart", "WidgetPart", "TitlePart" };

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
            var attributesToCompare = new string[] {"Title", "Text"};
            var elementsToCompare = new string[] {"BodyPart", "WidgetPart", "TitlePart"};

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
    }
}