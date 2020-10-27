using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TargetingTestApp.Grammar
{
    internal class NumericRuleExpressionGrammar : ParametizedExpressionGrammar<double, OperatorExtensions.None, FunctionExtensions.None>, IRuleExpressionParser
    {
        public NumericRuleExpressionGrammar(ILogger<NumericRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(double);
    }
}
