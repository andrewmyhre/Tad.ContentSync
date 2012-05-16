using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Tad.ContentSync.Services
{
    public interface ISynchronisationJobBuilder
    {
        void AddStep(string sourceItemIdentifier, string targetItemIdentifier);
        List<XElement> SynchronisationSteps { get; }
    }
}
