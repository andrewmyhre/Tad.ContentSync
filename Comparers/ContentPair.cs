using System.Xml.Linq;

namespace Tad.ContentSync.Comparers
{
    public class ContentPair
    {
        public ContentPair(XElement left, XElement right)
        {
            Left = left;
            Right = right;
        }

        public XElement Left { get; private set; }
        public XElement Right { get; private set; }
    }
}