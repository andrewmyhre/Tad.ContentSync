using System;
using System.Collections.Generic;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Recipes.Models;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public interface IRemoteContentFetchService: IDependency
    {
        Recipe FetchRecipe(Uri remoteInstanceRoot);
        IEnumerable<RemoteContentItem> Fetch(Uri remoteInstanceRoot);
    }
}
