using System;
using System.Collections.Generic;
using TargetingTestApp.Criterion;

namespace TargetingTestApp
{
    public static class TestCriteria
    {
        private static readonly Lazy<IEnumerable<ICriterion>> _allCriterion = new Lazy<IEnumerable<ICriterion>>(BuildCriteria);

        public static IEnumerable<ICriterion> All => _allCriterion.Value;

        private static IEnumerable<ICriterion> BuildCriteria()
        {
            var criteria = new List<ICriterion>
            {
                new Tag { ReferenceCode = "ABC" },
                new Tag { ReferenceCode = "DEF" },
                new Tag { ReferenceCode = "GHI" },
                new Segment { ReferenceCode = "SEG1", SegmentExpression = "(ABC AND DEF)" },
                new Rule { ReferenceCode = "Age20To50", EvaluationTarget = "Age", EvaluationType = typeof(double?), EvaluationCriterion = "(x > 20) AND (x <= 50)" },
                new Rule { ReferenceCode = "BenOffers", EvaluationTarget = "Name", EvaluationType = typeof(string), EvaluationCriterion = "x regexmatches \".*gh.*\"" },
                new Rule { ReferenceCode = "BornThisWeek", EvaluationTarget = "DateOfBirth", EvaluationType = typeof(DateTime?), EvaluationCriterion = "DayRangeFromDate(x, false) <= 7" }
            };
            return criteria;
        }
    }
}
