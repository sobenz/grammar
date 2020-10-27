using System;

namespace TargetingTestApp.Criterion
{
    public class Tag : ICriterion
    {
        public CriterionType CriteriaType => CriterionType.Simple;

        public string ReferenceCode { get; set; }
    }
}
