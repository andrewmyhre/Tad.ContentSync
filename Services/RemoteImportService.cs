using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Logging;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services {
    public class RemoteImportService : IRemoteImportService {
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        public ILogger Logger { get; set; }

        public RemoteImportService(
            ITransactionManager transactionManager,
            IContentManager contentManager,
            ILoggerFactory loggerFactory) {
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            Logger = loggerFactory.CreateLogger(this.GetType());
        }

        public void Import(IEnumerable<ImportSyncAction> actions, bool rollback) {
            _transactionManager.Demand();

            try
            {
                // process replacements
                var importContentSession = new ImportContentSession(_contentManager);
                foreach (var sync in actions.Where(a => a.Action == "Replace"))
                {
                    Logger.Debug("{0}, {1}", sync.Action, sync.TargetId);
                    if (!LocalIdentifierExists(sync.TargetId, importContentSession))
                    {
                        Replace(sync, importContentSession);
                    } else
                    {
                        throw new Exception(string.Format("{0} already exists, will not perform a replace import", sync.TargetId));
                    }
                }

                // import
                importContentSession = new ImportContentSession(_contentManager);
                foreach (var action in actions)
                {
                    ImportItem(action, importContentSession);
                    Logger.Debug("Imported a " + action.Step.Name);
                }

                if (rollback)
                {
                    _transactionManager.Cancel();
                    Logger.Debug("Rollback import transaction");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error importing", ex);
                _transactionManager.Cancel();
                throw;
            }
            finally {
            }
        }

        private bool LocalIdentifierExists(string identifier, ImportContentSession importContentSession)
        {
            var contentItem = importContentSession.Get(identifier);
            if (contentItem == null)
                return false;

            return true;
        }

        private void ImportItem(ImportSyncAction action, ImportContentSession session) {
            _contentManager.Import(action.Step.Step, session);
        }

        private void Replace(ImportSyncAction action, ImportContentSession session)
        {
            // update the identifier on the item in the local instance
            // then let the import continue so the existing item gets paved over
            if (action.Action == "Replace" && !string.IsNullOrWhiteSpace(action.TargetId))
            {
                var item = session.Get(action.TargetId);

                if (item == null)
                {
                    return;
                }

                var newIdentifier = action.Step.Step.Attribute("Id");
                if (newIdentifier == null)
                    return;

                var newIdentity = new ContentIdentity(newIdentifier.Value);
                var existingIdentity = new ContentIdentity(action.TargetId);
                if (!newIdentity.Equals(existingIdentity))
                {
                    Logger.Debug("import - items {0} and {1} have different identifiers", existingIdentity.Get("Identifier"), newIdentity.Get("Identifier"));

                    item.As<IdentityPart>().Identifier = newIdentity.Get("Identifier");
                    session.Store(newIdentity.Get("Identifier"), item);
                }
            }

        }
    }
}