using System;
using System.Linq.Expressions;
using TargetingTestApp.Evaluation;

namespace TargetingTestApp.Parser
{
    public interface ITargetExpressionParser
    {
        Expression<Func<ITargetEvaluator, bool>> GenerateExpression(string expression);
        bool ValidateExpression(string expression);
    }
}
