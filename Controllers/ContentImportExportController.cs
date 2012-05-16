using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Tad.ContentSync.Models;
using Tad.ContentSync.Services;

namespace Tad.ContentSync.Controllers
{
    public class ContentImportExportController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IRemoteImportService _remoteImportService;
        private readonly IOrchardServices _orchardServices;
        public readonly ILogger Logger = null;

        public ContentImportExportController(
            IContentManager contentManager,
            IRemoteImportService remoteImportService,
            ILoggerFactory loggerfactory,
            IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _remoteImportService = remoteImportService;
            _orchardServices = orchardServices;
            Logger = loggerfactory.CreateLogger(typeof(ContentImportExportController));
        }

        public ActionResult Export() {
            var content = _contentManager
                .Query(VersionOptions.Published)
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

    [HttpPost]
        public ActionResult Import() {

        Logger.Debug("Import requested by " + Request.UserHostAddress);
        bool rollback = false;

        string requestContent = "";

        if (Request.Headers.Get("Transaction-Rollback") != null)
        {
            bool.TryParse(Request.Headers.Get("Transaction-Rollback"), out rollback);
        }
        using (StreamReader reader = new StreamReader(Request.InputStream, Request.ContentEncoding)) {
            requestContent = reader.ReadToEnd();
        }


        XDocument xml = XDocument.Parse(requestContent);
            var syncSteps = xml.Element("ContentSync").Elements("Sync")
                .Select(e => ImportSyncAction.Parse(e));

            try
            {
                _remoteImportService.Import(syncSteps, rollback);
            } catch (Exception ex)
            {
                Logger.Error("There was an error while importing", ex);
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, ex.Message);
            }


        return new HttpStatusCodeResult((int)HttpStatusCode.Accepted);
        }

        public string TestImportXml()
        {
            return @"<?xml version=""1.0"" encoding=""utf-16""?>
<ContentSync>
<Sync Action=""Replace"" TargetId=""/Identifier=09735397de444495ad819968b5c98765/alias="">
	<Page
	  Id=""/Identifier=09735397de444495ad819968b5c98761/alias=""
	  Status=""Published"">
	  <IdentityPart
		Identifier=""09735397de444495ad819968b5c98761"" />
	  <BodyPart
		Text=""&lt;p&gt;You've successfully setup your Orchard Site and this is the homepage of your new site.&#xD;&#xA;Here are a few things you can look at to get familiar with the application.&#xD;&#xA;Once you feel confident you don't need this anymore, you can&#xD;&#xA;&lt;a href=&quot;Admin/Contents/Edit/11&quot;&gt;remove it by going into editing mode&lt;/a&gt;&#xD;&#xA;and replacing it with whatever you want.&lt;/p&gt;&#xD;&#xA;&lt;p&gt;First things first - You'll probably want to &lt;a href=&quot;Admin/Settings&quot;&gt;manage your settings&lt;/a&gt;&#xD;&#xA;and configure Orchard to your liking. After that, you can head over to&#xD;&#xA;&lt;a href=&quot;Admin/Themes&quot;&gt;manage themes to change or install new themes&lt;/a&gt;&#xD;&#xA;and really make it your own. Once you're happy with a look and feel, it's time for some content.&#xD;&#xA;You can start creating new custom content types or start from the built-in ones by&#xD;&#xA;&lt;a href=&quot;Admin/Contents/Create/Page&quot;&gt;adding a page&lt;/a&gt;, or &lt;a href=&quot;Admin/Navigation&quot;&gt;managing your menus.&lt;/a&gt;&lt;/p&gt;&#xD;&#xA;&lt;p&gt;Finally, Orchard has been designed to be extended. It comes with a few built-in&#xD;&#xA;modules such as pages and blogs or themes. If you're looking to add additional functionality,&#xD;&#xA;you can do so by creating your own module or by installing one that somebody else built.&#xD;&#xA;Modules are created by other users of Orchard just like you so if you feel up to it,&#xD;&#xA;&lt;a href=&quot;http://orchardproject.net/contribution&quot;&gt;please consider participating&lt;/a&gt;.&lt;/p&gt;&#xD;&#xA;&lt;p&gt;Thanks for using Orchard – The Orchard Team &lt;/p&gt;"" />
	  <CommonPart
		Owner=""/User.UserName=admin""
		CreatedUtc=""2012-05-04T18:20:39Z""
		PublishedUtc=""2012-05-04T18:20:39Z""
		ModifiedUtc=""2012-05-04T18:20:39Z"" />
	  <AutoroutePart
		Alias=""""
		CustomPattern=""""
		UseCustomPattern=""true"" />
	  <MenuPart
		MenuText=""""
		OnMainMenu=""false"" />
	  <TagsPart
		Tags="""" />
	  <TitlePart
		Title=""Welcome to Orchard!"" />
	</Page>
</Sync>
</ContentSync>";
        }


    }
}
