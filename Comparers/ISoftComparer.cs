using Orchard;

namespace Tad.ContentSync.Comparers
{
    // inject ISoftComparer to get comparers which work on content, ignoring identifiers. 
    // used to find similarities and therefore potential mismatches
    public interface ISoftComparer : IRecipeStepComparer
    {
    }
}