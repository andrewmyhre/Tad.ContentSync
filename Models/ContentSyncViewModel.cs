using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tad.ContentSync.Comparers;

namespace Tad.ContentSync.Models
{
    public class ContentSyncViewModel
    {
        public IList<ContentPair> Pairs { get; set; }
        public DateTime GeneratedOn { get; set; }

        public string RemoteServerUrl { get; set; }

        public IList<IGrouping<XElement, ContentPair>> SimilarSets { get; set; }

        public IList<ContentPair> Same { get; set; }

        public IList<ContentPair> Different { get; set; }
    }
}