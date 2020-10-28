using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Grammar for rules that are based on Numeric properties.
    /// </summary>
    /// <remarks>All numeric values are treated as nullable doubles.</remarks>
    internal class NumericRuleExpressionGrammar : ParametizedExpressionGrammar<double?, OperatorExtensions.None, FunctionExtensions.None>, IRuleExpressionParser
    {
        public NumericRuleExpressionGrammar(ILogger<NumericRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(double?);
    }
}
