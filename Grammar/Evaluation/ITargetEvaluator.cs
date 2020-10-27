using System;
using System.Collections.Generic;
using System.Text;

namespace TargetingTestApp.Evaluation
{
    public interface ITargetEvaluator
    {
        bool HasTag(string referenceCode);

        object GetRuleTarget(string name);
    }
}
