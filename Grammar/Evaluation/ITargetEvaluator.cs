using System;
using System.Collections.Generic;
using System.Text;

namespace TargetingTestApp.Evaluation
{
    /// <summary>
    /// Defines access to specific properties on an entity that is being evaluated for targeting.
    /// </summary>
    public interface ITargetEvaluator
    {
        /// <summary>
        /// Determines if the entity has a specific tag assigned to it.
        /// </summary>
        /// <param name="referenceCode">The reference code of the pipeline.</param>
        /// <returns></returns>
        bool HasTag(string referenceCode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetRuleTarget(string name);
    }
}
