using System.Linq;
using Tad.ContentSync.Models;

namespace Tad.ContentSync.Services
{
    public class PullSynchronisationJobRunner : ISynchronisationJobRunner
    {
        private IRemoteImportService _importService;
        public bool Rollback { get; set; }

        public PullSynchronisationJobRunner(IRemoteImportService importService)
        {
            _importService = importService;
        }

        public void Process(ISynchronisationJobBuilder syncJobBuilder)
        {
            var importActions = syncJobBuilder.SynchronisationSteps.Select(ImportSyncAction.Parse);
            _importService.Import(importActions, Rollback);
        }

        
    }
}