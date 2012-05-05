using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using ClaySharp;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;

namespace ContentSync
{
    public class Shapes : IDependency
    {
        [Shape]
        public void TestShape(dynamic Display, TextWriter Output, HtmlHelper Html, ContentItem item) {
            if (item.Has<IdentityPart>()) {
                Output.Write("testshape: " + item.As<IdentityPart>().Identifier);
            }
        }
    }
}
