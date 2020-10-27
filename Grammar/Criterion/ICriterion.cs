using System;

namespace TargetingTestApp.Criterion
{
    public interface ICriterion
    {
        CriterionType CriteriaType { get; }
        string ReferenceCode { get; }
    }
}
