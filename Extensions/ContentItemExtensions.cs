using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Tad.ContentSync.Services;

namespace Tad.ContentSync.Extensions {
    public static class ContentItemExtensions {
        public static bool IsEqualTo(this ContentItem o1, ContentItem o2, IContentManager contentManager) {
            //todo: this is a little too generous

            if(o1.Has<IdentityPart>() && o2.Has<IdentityPart>()) {
                if (!o1.As<IdentityPart>().Identifier.Equals(o2.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if(o1.Has<TitlePart>() && o2.Has<TitlePart>()) {
                if (!o1.As<TitlePart>().Title.Equals(o2.As<TitlePart>().Title, StringComparison.CurrentCulture)) {
                    return false;
                }
            }

            if (o1.Has<BodyPart>() && o2.Has<BodyPart>())
            {
                var text1 = o1.As<BodyPart>().Text;
                var text2 = o2.As<BodyPart>().Text;

                if (text1 == null || text2 == null)
                    return false;

                if (!o1.As<BodyPart>().Text.Equals(o2.As<BodyPart>().Text, StringComparison.CurrentCulture)) {
                    return false;
                }
            }

            var common1 = o1.As<CommonPart>();
            var common2 = o2.As<CommonPart>();
            if (!common1.ModifiedUtc.Equals(common2.ModifiedUtc)) {
                return false;
            }

            // compare xml elements
            var export1 = contentManager.Export(o1);
            var export2 = contentManager.Export(o2);

            var attributesToCompare = new string[] {"Title", "Text"};
            var elementsToCompare = new string[]{"BodyPart", "WidgetPart", "TitlePart"};

            foreach(var element in export1.Elements()) {
                if (!elementsToCompare.Contains(element.Name.LocalName))
                    continue;

                var counterpart = export2.Element(element.Name.LocalName);
                if (counterpart == null)
                    return false;
                foreach (var attributeName in attributesToCompare) {
                    if (element.Attribute(attributeName) != null &&
                        counterpart.Attribute(attributeName) != null)
                    {
                        if (!element.Attribute(attributeName).Value.Equals(counterpart.Attribute(attributeName).Value))
                            return false;
                    }
                    
                }
            }

            return true;
        }

        public static bool IsEqualTo(this ContentItem o1, RemoteContentItem remoteItem, IContentManager contentManager)
        {
            //todo: this is a little too generous
            var o2 = remoteItem.ContentItem;
            if (o1.Has<IdentityPart>() && o2.Has<IdentityPart>())
            {
                if (!o1.As<IdentityPart>().Identifier.Equals(o2.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if (o1.Has<TitlePart>() && o2.Has<TitlePart>())
            {
                if (!o1.As<TitlePart>().Title.Equals(o2.As<TitlePart>().Title, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            if (o1.Has<BodyPart>() && o2.Has<BodyPart>())
            {
                var text1 = o1.As<BodyPart>().Text;
                var text2 = o2.As<BodyPart>().Text;

                if (text1 == null || text2 == null)
                    return false;

                if (!o1.As<BodyPart>().Text.Equals(o2.As<BodyPart>().Text, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            var common1 = o1.As<CommonPart>();
            var common2 = o2.As<CommonPart>();
            if (!common1.ModifiedUtc.Equals(common2.ModifiedUtc))
            {
                return false;
            }

            // compare xml elements
            var export1 = contentManager.Export(o1);
            var export2 = remoteItem.Xml;

            var attributesToCompare = new string[] { "Title", "Text" };
            var elementsToCompare = new string[] { "BodyPart", "WidgetPart", "TitlePart" };

            foreach (var element in export1.Elements())
            {
                if (!elementsToCompare.Contains(element.Name.LocalName))
                    continue;

                var counterpart = export2.Element(element.Name.LocalName);
                if (counterpart == null)
                    return false;
                foreach (var attributeName in attributesToCompare)
                {
                    if (element.Attribute(attributeName) != null &&
                        counterpart.Attribute(attributeName) != null)
                    {
                        if (!element.Attribute(attributeName).Value.Equals(counterpart.Attribute(attributeName).Value))
                            return false;
                    }

                }
            }

            return true;
        }

        public static bool SimilarTo(this ContentItem ci1, ContentItem ci2) {
            if (ci1.ContentType.Equals(ci2.ContentType))
            {
                if (ci1.Has<TitlePart>() && ci2.Has<TitlePart>())
                {
                    if (!string.IsNullOrWhiteSpace(ci1.As<TitlePart>().Title))
                    {
                        if (ci1.As<TitlePart>().Title.Equals(ci2.As<TitlePart>().Title,
                                                             StringComparison.InvariantCultureIgnoreCase))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}