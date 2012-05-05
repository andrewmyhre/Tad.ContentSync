using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace ContentSync.Controllers
{
    public class ContentImportExportController : Controller
    {
        private readonly IContentManager _contentManager;

        public ContentImportExportController(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ActionResult Export() {
            var content = _contentManager
                .Query(VersionOptions.Latest)
                .Join<IdentityPartRecord>()
                .List();

            XDocument export = new XDocument();
            export.Add(new XElement("Orchard"));
            export.Element("Orchard").Add(new XElement("Data"));
            foreach(var contentItem in content) {
                export.Element("Orchard").Element("Data").Add(_contentManager.Export(contentItem));
            }

            StringBuilder xml = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings()
                {Encoding = Encoding.UTF8, Indent = true, NewLineHandling = NewLineHandling.Entitize, NewLineOnAttributes = true};
            using (XmlWriter w = XmlWriter.Create(xml, settings)) {
                export.WriteTo(w);
                w.Flush();
            }

            return new ContentResult() {
                Content = xml.ToString(),
                ContentType = "text/xml",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
