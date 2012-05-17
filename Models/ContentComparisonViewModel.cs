using System.Collections.Generic;
using System.Linq;
using Tad.ContentSync.Comparers;

namespace Tad.ContentSync.Models
{
    public class ContentComparisonViewModel
    {
        public string RemoteServerUrl { get;set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement,ContentPair>>> Comparison { get; set; }
    }
}