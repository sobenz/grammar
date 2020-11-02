using System;
using System.Collections.Generic;
using TargetingTestApp.Consumer;
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
                new Tag { ReferenceCode = "SampleGroup1" },
                new Tag { ReferenceCode = "SampleGroup2" },
                new Tag { ReferenceCode = "MeatLover" },
                new Tag { ReferenceCode = "Vegetarian" },
                new Segment { ReferenceCode = "VegetarianTestSegment", SegmentExpression = "Vegetarian OR (SampleGroup1 AND MeatLover)" },
                new Rule { ReferenceCode = "Age20To50", EvaluationTarget = "Age", EvaluationType = typeof(double?), EvaluationCriterion = "(x > 20) AND (x <= 50)" },
                new Rule { ReferenceCode = "BensOffers", EvaluationTarget = "Name", EvaluationType = typeof(string), EvaluationCriterion = "x startswith \"Ben\"" },
                new Rule { ReferenceCode = "BornThisWeek", EvaluationTarget = "DateOfBirth", EvaluationType = typeof(DateTime?), EvaluationCriterion = "DayRangeFromDate(x, false) <= 7" },
                new Rule { ReferenceCode = "NZMarket", EvaluationTarget = "CurrentMarket", EvaluationType = typeof(string), EvaluationCriterion = "x == \"New Zealand\"" },
                new Rule { ReferenceCode = "2RedeptionsLast7DaysNZ", EvaluationTarget = "Redemptions", EvaluationType=typeof(IEnumerable<ConsumerEvent>), EvaluationCriterion = "EventsInDayRange(ForMarket(x,\"New Zealand\"),7) >= 2" },
                new Rule { ReferenceCode = "2RedeptionsLast14DaysNZ", EvaluationTarget = "Redemptions", EvaluationType=typeof(IEnumerable<ConsumerEvent>), EvaluationCriterion = "EventsInDayRange(ForMarket(x,\"New Zealand\"),14) >= 2" },
                new Rule { ReferenceCode = "2RedeptionsLast31DaysNZ", EvaluationTarget = "Redemptions", EvaluationType=typeof(IEnumerable<ConsumerEvent>), EvaluationCriterion = "EventsInDayRange(ForMarket(x,\"New Zealand\"),31) >= 2" },
            };
            return criteria;
        }
    }
}
