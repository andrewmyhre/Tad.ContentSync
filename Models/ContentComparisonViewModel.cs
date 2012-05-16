using System.Collections.Generic;
using Tad.ContentSync.Comparers;

namespace Tad.ContentSync.Models
{
    public class ContentComparisonViewModel
    {
        public IEnumerable<ContentPair> Pairs { get; set; }
    }
}