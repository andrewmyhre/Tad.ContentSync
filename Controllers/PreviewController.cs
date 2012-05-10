using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Tad.ContentSync.Controllers
{
    public class PreviewController : Controller
    {
        private readonly IContentManager _contentManager;

        public PreviewController(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public ActionResult Index(string identifier)
        {
            int contentItemId = 0;
            ContentItem item = null;

            dynamic shape;
            if (int.TryParse(identifier, out contentItemId))
            {
                item = _contentManager.Get(contentItemId);
            }
            else
            {
                item = _contentManager.Query()
                    .Join<IdentityPartRecord>()
                    .Where(ip => ip.Identifier == identifier)
                    .Slice(0, 1)
                    .SingleOrDefault();

            }

            if (item != null)
            {
                shape = _contentManager.BuildDisplay(item);
                return View(shape);
            }

            return HttpNotFound();
        }
    }
}
