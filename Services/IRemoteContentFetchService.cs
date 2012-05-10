using System;
using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public interface IRemoteContentFetchService: IDependency {
        IEnumerable<RemoteContentItem> Fetch(Uri remoteInstanceRoot);
    }
}
