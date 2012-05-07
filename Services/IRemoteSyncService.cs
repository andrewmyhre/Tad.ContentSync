using System;
using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public interface IRemoteSyncService: IDependency {
        IEnumerable<ContentItem> Fetch(Uri remoteInstanceRoot);
        IEnumerable<ContentSyncMap> GenerateSynchronisationMappings(IEnumerable<ContentItem> localContent, IEnumerable<ContentItem> remoteContents);
    }
}
