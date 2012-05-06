using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentSync.Models;
using Orchard;
using Orchard.ContentManagement;

namespace ContentSync.Services
{
    public interface IRemoteSyncService: IDependency {
        IEnumerable<ContentItem> Fetch(Uri remoteInstanceRoot);
        IEnumerable<ContentSyncMap> GenerateSynchronisationMappings(IEnumerable<ContentItem> remoteContents);
    }
}
