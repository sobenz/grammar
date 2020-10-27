using System;
using System.Collections.Generic;
using System.Text;

namespace TargetingTestApp.Grammar
{
    public interface IRuleExpressionParser : IExpressionParser
    {
        Type RuleParameterType { get; }
    }
}
