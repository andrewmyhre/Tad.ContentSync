using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public interface ISynchronisationMapFactory : IDependency
    {
        IEnumerable<ContentSyncMap> BuildSynchronisationMap(IEnumerable<ContentItem> localContent, IEnumerable<ContentItem> remoteContents);
    }
}