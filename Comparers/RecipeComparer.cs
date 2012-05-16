using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Tad.ContentSync.Comparers
{
    public class RecipeComparer
    {
        public RecipeComparisonResult Compare(Recipe left, Recipe right, Func<XElement, XElement, bool> comparison)
        {
            var leftParts = left.RecipeSteps.Where(s => s.Name == "Data").SingleOrDefault().Step.Elements();
            var rightParts = right.RecipeSteps.Where(s => s.Name == "Data").SingleOrDefault().Step.Elements();

            var matching = new List<ContentPair>();
            var unmatched = new List<ContentPair>();
            bool matched = false;

            // enumerate content items
            for (int leftIndex = 0; leftIndex < leftParts.Count(); leftIndex++)
            {
                matched = false;
                var leftPart = leftParts.ElementAt(leftIndex);
                for (int rightIndex = 0; rightIndex < rightParts.Count(); rightIndex++)
                {
                    var rightPart = rightParts.ElementAt(rightIndex);
                    if (comparison(leftPart, rightPart))
                    {
                        matching.Add(new ContentPair(leftPart, rightPart));
                        matched = true;
                    }
                }
                if (!matched)
                {
                    unmatched.Add(new ContentPair(leftPart, null));
                }
            }

            unmatched.AddRange(
                rightParts.Where(part => !matching.Any(pair => pair.Right == part))
                .Select(part => new ContentPair(null, part)));

            return new RecipeComparisonResult(matching, unmatched);
        }

        public RecipeComparisonResult Compare(Recipe left, Recipe right, params IRecipeStepComparer[] comparers)
        {
            var leftParts = left.RecipeSteps.Where(s => s.Name == "Data").SingleOrDefault().Step.Elements();
            var rightParts = right.RecipeSteps.Where(s => s.Name == "Data").SingleOrDefault().Step.Elements();

            var matching = new List<ContentPair>();
            var unmatched = new List<ContentPair>();
            bool matched = false;

            // enumerate content items
            for (int leftIndex = 0; leftIndex < leftParts.Count(); leftIndex++)
            {
                matched = false;
                var leftPart = leftParts.ElementAt(leftIndex);
                for (int rightIndex = 0; rightIndex < rightParts.Count(); rightIndex++)
                {
                    var rightPart = rightParts.ElementAt(rightIndex);
                    bool areDifferent = false;
                    foreach (var comparer in comparers)
                    {
                        if (!comparer.IsMatch(leftPart, rightPart))
                        {
                            areDifferent = true;
                            break;
                        }
                    }
                    if (!areDifferent)
                    {
                        matching.Add(new ContentPair(leftPart, rightPart));
                        matched = true;
                    }
                }
                if (!matched)
                {
                    unmatched.Add(new ContentPair(leftPart, null));
                }
            }

            unmatched.AddRange(
                rightParts.Where(part => !matching.Any(pair => pair.Right == part))
                .Select(part => new ContentPair(null, part)));

            return new RecipeComparisonResult(matching, unmatched);
        }
    }
}