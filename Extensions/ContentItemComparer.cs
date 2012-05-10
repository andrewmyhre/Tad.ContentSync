using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using System.Linq;

namespace Tad.ContentSync.Extensions
{
    public class ContentItemComparer
    {
        private readonly ContentItem _contentItem;
        private readonly IContentManager _contentManager;

        public ContentItemComparer(ContentItem contentItem, IContentManager contentManager)
        {
            _contentItem = contentItem;
            _contentManager = contentManager;
        }

        public bool EqualTo(ContentItem other)
        {
            if (_contentItem.Has<IdentityPart>() && other.Has<IdentityPart>())
            {
                if (!_contentItem.As<IdentityPart>().Identifier.Equals(other.As<IdentityPart>().Identifier, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            if (_contentItem.Has<TitlePart>() && other.Has<TitlePart>())
            {
                if (!_contentItem.As<TitlePart>().Title.Equals(other.As<TitlePart>().Title, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            if (_contentItem.Has<BodyPart>() && other.Has<BodyPart>())
            {
                var text1 = _contentItem.As<BodyPart>().Text;
                var text2 = other.As<BodyPart>().Text;

                if (text1 == null || text2 == null)
                    return false;

                if (!_contentItem.As<BodyPart>().Text.Equals(other.As<BodyPart>().Text, StringComparison.CurrentCulture))
                {
                    return false;
                }
            }

            var common1 = _contentItem.As<CommonPart>();
            var common2 = other.As<CommonPart>();
            if (common1.ModifiedUtc.HasValue && common2.ModifiedUtc.HasValue
                        && common1.ModifiedUtc.Value > common2.ModifiedUtc.Value)
            {
                return false;
            }

            // compare xml elements
            var export1 = _contentManager.Export(_contentItem);
            var export2 = _contentManager.Export(other);

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

        public int SimilarityTo(ContentItem other)
        {
            if (_contentItem.ContentType.Equals(other.ContentType))
            {
                if (_contentItem.Has<TitlePart>() && other.Has<TitlePart>())
                {
                    if (!string.IsNullOrWhiteSpace(_contentItem.As<TitlePart>().Title))
                    {
                        if (_contentItem.As<TitlePart>().Title.Equals(other.As<TitlePart>().Title,
                                                             StringComparison.InvariantCultureIgnoreCase))
                            return 1;
                    }

                    // compare xml elements
                    int similarity=0;
                    var export1 = _contentManager.Export(_contentItem);
                    var export2 = _contentManager.Export(other);

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
                                if (element.Attribute(attributeName).Value.Equals(counterpart.Attribute(attributeName).Value))
                                    similarity++;
                            }

                        }
                        return similarity;
                    }


                }
            }

            return 0;
        }
    }
}