using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tad.ContentSync.Comparers;

namespace Tad.ContentSync.Models
{
    public class OverviewViewModel
    {
        public DateTime GeneratedOn { get; set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement, ContentPair>>> LocalOnly { get; set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement, ContentPair>>> Same { get; set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement, ContentPair>>> Different { get; set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement, ContentPair>>> RemoteOnly { get; set; }
        public IEnumerable<IGrouping<string, IGrouping<System.Xml.Linq.XElement, ContentPair>>> Mismatches { get; set; }
        public string RemoteServerUrl { get; set; }
    }
}