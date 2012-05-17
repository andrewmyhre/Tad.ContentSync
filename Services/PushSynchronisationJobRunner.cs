using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Tad.ContentSync.Services
{
    public class PushSynchronisationJobRunner : ISynchronisationJobRunner
    {
        private readonly IOrchardServices _services;
        private readonly ISignals _signals;
        public string RemoteInstanceUrl { get; set; }
        public bool Rollback { get; set; }

        public PushSynchronisationJobRunner(string remoteInstanceUrl, IOrchardServices services, ISignals signals)
        {
            RemoteInstanceUrl = remoteInstanceUrl;
            _services = services;
            _signals = signals;
        }

        public void Process(ISynchronisationJobBuilder syncJobBuilder)
        {
            if (string.IsNullOrWhiteSpace(RemoteInstanceUrl))
            {
                throw new InvalidOperationException("RemoteInstanceUrl property must be set before running the PUsh Synchronisation Job RUnner");
            }

            XDocument synchronisation = new XDocument(
                new XElement("ContentSync", syncJobBuilder.SynchronisationSteps));

            // send to other server
            string remoteImportEndpoint = RemoteInstanceUrl + "/Admin/ContentImportExport/Import";
            HttpWebRequest post = HttpWebRequest.Create(remoteImportEndpoint) as HttpWebRequest;
            post.Method = "POST";
            post.Headers.Add("Transaction-Rollback", Rollback.ToString());
            using (var requestStream = post.GetRequestStream())
            {
                using (var requestWriter = new StreamWriter(requestStream, Encoding.UTF8))
                {
                    requestWriter.Write(synchronisation.ToString());
                    requestWriter.Flush();
                }
            }

            try
            {
                using (var response = post.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        _services.Notifier.Add(NotifyType.Information,
                                               new LocalizedString("The content was published successfully"));
                        _signals.Trigger(SynchronisationMapFactory.MapInvalidationTrigger);
                    }
                    else
                    {
                        _services.Notifier.Add(NotifyType.Error,
                                               new LocalizedString("A problem occurred: " + response.StatusDescription));
                    }
                }
            }
            catch (Exception ex)
            {
                _services.Notifier.Add(NotifyType.Error,
                                       new LocalizedString("A problem occurred: " + ex.Message));
            }
        }

        
    }
}