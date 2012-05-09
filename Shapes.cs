using System.IO;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;

namespace Tad.ContentSync
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

    public class ShapeDescriptor : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Layer")
                .OnDisplaying(a => a.Shape.Name("layer name goes here?"));
        }
    }
}
