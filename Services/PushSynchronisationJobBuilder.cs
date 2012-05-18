using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Tad.ContentSync.Services
{
    public class PushSynchronisationJobBuilder : ISynchronisationJobBuilder
    {
        private readonly ImportContentSession _importContentSession;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly ISignals _signals;
        private List<XElement> _synchronisationSteps = new List<XElement>();

        public PushSynchronisationJobBuilder(
            ImportContentSession importContentSession, 
            IContentManager contentManager, 
            IOrchardServices services,
            ISignals signals)
        {
            _importContentSession = importContentSession;
            _contentManager = contentManager;
            _services = services;
            _signals = signals;
        }

        public void Process(string remoteInstanceUrl, bool rollback)
        {
            XDocument synchronisation = new XDocument(
                new XElement("ContentSync", _synchronisationSteps));

            // send to other server
            string remoteImportEndpoint = remoteInstanceUrl + "/Admin/ContentImportExport/Import";
            HttpWebRequest post = HttpWebRequest.Create(remoteImportEndpoint) as HttpWebRequest;
            post.Method = "POST";
            post.Headers.Add("Transaction-Rollback", rollback.ToString());
            using (var requestStream = post.GetRequestStream())
            {
                using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8))
                {
                    requestWriter.Write(synchronisation.ToString());
                    requestWriter.Flush();
                }
            }

            try {
                using (var response = post.GetResponse() as HttpWebResponse) {
                    if (response.StatusCode == HttpStatusCode.Accepted) {
                        _services.Notifier.Add(NotifyType.Information, 
                            new LocalizedString("The content was published successfully"));
                        _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);
                    } else {
                        _services.Notifier.Add(NotifyType.Error,
                            new LocalizedString("A problem occurred: " + response.StatusDescription));
                    }
                }
            } catch (Exception ex) {
                _services.Notifier.Add(NotifyType.Error,
                    new LocalizedString("A problem occurred: " + ex.Message));
            }
        }

        public void AddStep(string localIdentifier, string remoteIdentifier)
        {
            var contentItem = _importContentSession.Get(localIdentifier);
            XElement sync = new XElement("Sync");
            if (!localIdentifier.Equals(remoteIdentifier))
            {
                sync.Add(new XAttribute("Action", "Replace"));
                sync.Add(new XAttribute("TargetId", remoteIdentifier));
                sync.Add(_contentManager.Export(contentItem));
            }
            else
            {
                sync.Add(new XAttribute("Action", "Import"));
                sync.Add(_contentManager.Export(contentItem));
            }
            _synchronisationSteps.Add(sync);
        }
    }
}