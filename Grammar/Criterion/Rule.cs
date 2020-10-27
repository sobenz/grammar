using System;

namespace TargetingTestApp.Criterion
{
    public class Rule : ICriterion
    {
        public CriterionType CriteriaType => CriterionType.Rule;

        public string ReferenceCode { get; set; }

        public string EvaluationTarget { get; set; }

        public Type EvaluationType { get; set; }

        public string EvaluationCriterion { get; set; }
    }
}
