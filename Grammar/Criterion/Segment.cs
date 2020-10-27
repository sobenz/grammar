using System;

namespace TargetingTestApp.Criterion
{
    public class Segment : ICriterion
    {
        public CriterionType CriteriaType => CriterionType.Segment;

        public string ReferenceCode { get; set; }

        public string SegmentExpression { get; set; }
    }
}
