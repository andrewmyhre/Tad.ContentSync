using System.Xml.Linq;
using Orchard.ContentManagement;

namespace Tad.ContentSync.Services
{
    public class RemoteContentItem
    {
        public ContentItem ContentItem { get; set; }
        public XElement Xml { get; set; }
    }
}