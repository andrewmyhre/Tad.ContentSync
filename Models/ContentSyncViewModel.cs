using System;
using System.Collections.Generic;

namespace Tad.ContentSync.Models
{
    public class ContentSyncViewModel
    {
        public IEnumerable<ContentSyncMap> Mappings { get; set; }
        public DateTime GeneratedOn { get; set; }

        public string RemoteServerUrl { get; set; }
    }
}