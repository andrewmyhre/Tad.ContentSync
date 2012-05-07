using System.Collections.Generic;
using Orchard;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services {
    public interface IRemoteImportService : IDependency {
        void Import(IEnumerable<ImportSyncAction> actions);
    }
}