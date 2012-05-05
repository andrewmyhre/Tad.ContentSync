using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace ContentSync.Services {
    public class RemoteImportService : IRemoteImportService {
        private readonly IRecipeParser _recipeParser;
        private readonly IOrchardServices _orchardServices;
        private readonly Lazy<IEnumerable<IContentHandler>> _handlers;

        public RemoteImportService(
            IRecipeParser recipeParser,
            IOrchardServices orchardServices,
            Lazy<IEnumerable<IContentHandler>> handlers)
        {
            _recipeParser = recipeParser;
            _orchardServices = orchardServices;
            _handlers = handlers;
        }

        public IEnumerable<ContentItem> Fetch(Uri remoteInstanceRoot) {
            var remoteExportEndpoint = new Uri(remoteInstanceRoot + "/Admin/ContentImportExport/Export");
            string remoteXml = FetchRemoteExportXml(remoteExportEndpoint);
            List<ContentItem> contentItems = new List<ContentItem>();

            var recipe = _recipeParser.ParseRecipe(remoteXml);
            var importContentSession = new ImportContentSession(_orchardServices.ContentManager);
            foreach (var step in recipe.RecipeSteps)
            {
                foreach (var element in step.Step.Elements())
                {
                    var elementId = element.Attribute("Id");
                    if (elementId == null)
                        continue;

                    var identity = elementId.Value;
                    var status = element.Attribute("Status");

                    var item = importContentSession.Get(identity);
                    if (item == null)
                    {
                        item = _orchardServices.ContentManager.New(element.Name.LocalName);
                    }
                    else
                    {
                        item = _orchardServices.ContentManager.Get(item.Id, VersionOptions.DraftRequired);
                    }

                    var context = new ImportContentContext(item, element, importContentSession);

                    foreach (var contentHandler in _handlers.Value)
                    {
                        contentHandler.Importing(context);
                    }

                    foreach (var contentHandler in _handlers.Value)
                    {
                        contentHandler.Imported(context);
                    }


                    contentItems.Add(item);
                }
            }

            return contentItems;
        }

        private string FetchRemoteExportXml(Uri remoteExportUrl) {
            XDocument xml = XDocument.Load(remoteExportUrl.AbsoluteUri);
            return xml.ToString();
        }

        private string TestXml(Uri remoteInstanceRoot)
        {
            return @"<?xml version=""1.0"" encoding=""utf-16""?>
<Orchard>
  <Data>
    <HtmlWidget
      Id=""/Identifier=SetupHtmlWidget1""
      Status=""Published"">
      <IdentityPart
        Identifier=""SetupHtmlWidget1"" />
      <BodyPart
        Text=""&lt;p&gt;Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.&lt;/p&gt;"" />
      <CommonPart
        Owner=""/User.UserName=admin""
        Container=""/Layer.LayerName=TheHomepage""
        CreatedUtc=""2012-05-04T18:20:39Z""
        PublishedUtc=""2012-05-04T18:20:39Z""
        ModifiedUtc=""2012-05-04T18:20:39Z"" />
      <WidgetPart
        Title=""First Leader Aside""
        Position=""5""
        Zone=""TripelFirst""
        RenderTitle=""true"" />
    </HtmlWidget>
    <HtmlWidget
      Id=""/Identifier=SetupHtmlWidget2""
      Status=""Published"">
      <IdentityPart
        Identifier=""SetupHtmlWidget2"" />
      <BodyPart
        Text=""&lt;p&gt;Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.&lt;/p&gt;"" />
      <CommonPart
        Owner=""/User.UserName=admin""
        Container=""/Layer.LayerName=TheHomepage""
        CreatedUtc=""2012-05-04T18:20:39Z""
        PublishedUtc=""2012-05-04T18:20:39Z""
        ModifiedUtc=""2012-05-04T18:20:39Z"" />
      <WidgetPart
        Title=""Second Leader Aside""
        Position=""5""
        Zone=""TripelSecond""
        RenderTitle=""true"" />
    </HtmlWidget>
    <HtmlWidget
      Id=""/Identifier=SetupHtmlWidget3""
      Status=""Published"">
      <IdentityPart
        Identifier=""SetupHtmlWidget3"" />
      <BodyPart
        Text=""&lt;p&gt;Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur a nibh ut tortor dapibus vestibulum. Aliquam vel sem nibh. Suspendisse vel condimentum tellus.&lt;/p&gt;"" />
      <CommonPart
        Owner=""/User.UserName=admin""
        Container=""/Layer.LayerName=TheHomepage""
        CreatedUtc=""2012-05-04T18:20:39Z""
        PublishedUtc=""2012-05-04T18:20:39Z""
        ModifiedUtc=""2012-05-04T18:20:39Z"" />
      <WidgetPart
        Title=""Third Leader Aside""
        Position=""5""
        Zone=""TripelThird""
        RenderTitle=""true"" />
    </HtmlWidget>
    <Page
      Id=""/Identifier=32e945dd387a46f8844c396c760d4c90/alias=""
      Status=""Published"">
      <IdentityPart
        Identifier=""32e945dd387a46f8844c396c760d4c90"" />
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
    <Page
      Id=""/Identifier=32e945dd387a46f8844c396c760d4666/alias=""
      Status=""Published"">
      <IdentityPart
        Identifier=""32e945dd387a46f8844c396c760d4666"" />
      <BodyPart
        Text=""&lt;p&gt;this is not a real page&lt;/p&gt;"" />
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
        Title=""NOT A REAL PAGE"" />
    </Page>
    <Blog
      Id=""/Identifier=7954a38cbeb54c03af253f793f67fa71/alias=my-blog""
      Status=""Published"">
      <IdentityPart
        Identifier=""7954a38cbeb54c03af253f793f67fa71"" />
      <CommonPart
        Owner=""/User.UserName=admin""
        CreatedUtc=""2012-05-05T12:14:38Z""
        PublishedUtc=""2012-05-05T12:14:39Z""
        ModifiedUtc=""2012-05-05T12:14:39Z"" />
      <AutoroutePart
        Alias=""my-blog""
        UseCustomPattern=""false"" />
      <AdminMenuPart
        AdminMenuText=""blog""
        AdminMenuPosition=""2""
        OnAdminMenu=""true"" />
      <MenuPart
        MenuText=""blog""
        MenuPosition=""2""
        OnMainMenu=""true"" />
      <BlogPart
        Description=""this is my blog""
        PostCount=""1"" />
      <TitlePart
        Title=""My Blog"" />
    </Blog>
    <BlogPost
      Id=""/Identifier=22e6858b5a784411a4ddffdd9e117429/alias=my-blog\/first-post""
      Status=""Published"">
      <IdentityPart
        Identifier=""22e6858b5a784411a4ddffdd9e117429"" />
      <BodyPart
        Text=""&lt;p&gt;thisis theforst post on my blog&lt;/p&gt;"" />
      <CommonPart
        Owner=""/User.UserName=admin""
        Container=""/Identifier=7954a38cbeb54c03af253f793f67fa71/alias=my-blog""
        CreatedUtc=""2012-05-05T12:14:57Z""
        PublishedUtc=""2012-05-05T12:14:57Z""
        ModifiedUtc=""2012-05-05T12:14:57Z"" />
      <AutoroutePart
        Alias=""my-blog/first-post""
        UseCustomPattern=""false"" />
      <CommentsPart
        CommentsShown=""true""
        CommentsActive=""true"" />
      <TagsPart
        Tags="""" />
      <TitlePart
        Title=""first post"" />
    </BlogPost>
  </Data>
</Orchard>";
        }
    }
}