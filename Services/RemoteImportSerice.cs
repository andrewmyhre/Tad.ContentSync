using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Logging;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services {
    public class RemoteImportSerice : IRemoteImportService {
        private readonly ITransactionManager _transactionManager;
        private readonly IContentManager _contentManager;
        public ILogger Logger { get; set; }

        public RemoteImportSerice(
            ITransactionManager transactionManager,
            IContentManager contentManager,
            ILoggerFactory loggerFactory) {
            _transactionManager = transactionManager;
            _contentManager = contentManager;
            Logger = loggerFactory.CreateLogger(this.GetType());
        }

        public void Import(IEnumerable<ImportSyncAction> actions) {
            _transactionManager.Demand();
            Logger.Debug("Beginning import");

            try
            {
                // process replacements
                var importContentSession = new ImportContentSession(_contentManager);
                foreach (var sync in actions.Where(a => a.Action == "Replace"))
                {
                    Logger.Debug("{0}, {1}", sync.Action, sync.TargetId);
                    Replace(sync, importContentSession);
                }

                // import
                importContentSession = new ImportContentSession(_contentManager);
                foreach (var action in actions)
                {
                    ImportItem(action, importContentSession);
                    Logger.Debug("Imported " + action.Step.Name);
                }
            }
            catch (Exception ex)
            {
                _transactionManager.Cancel();
                throw;
            }
            finally {
                
            }
        }

        private void ImportItem(ImportSyncAction action, ImportContentSession session) {
            Logger.Debug("Importing {0}", action.Step.Step.Element("Id"));
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