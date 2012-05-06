using System.Collections.Generic;
using ContentSync.Models;
using Orchard;

namespace ContentSync.Services {
    public interface IRemoteImportService : IDependency {
        void Import(IEnumerable<ImportSyncAction> actions);
    }
}