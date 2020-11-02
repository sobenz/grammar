using System;
using System.Linq.Expressions;
using TargetingTestApp.Evaluation;
using TargetingTestApp.Grammar;

namespace TargetingTestApp.Parser
{
    internal class TargetExpressionParser : ITargetExpressionParser
    {
        private readonly TargetingExpressionGrammar _grammar;

        public TargetExpressionParser(TargetingExpressionGrammar grammar)
        {
            _grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
        }

        #region ITargetExpressionParser
        public Expression<Func<ITargetEvaluator, bool>> GenerateExpression(string expression)
        {
            return (Expression<Func<ITargetEvaluator, bool>>)_grammar.GenerateExpression(expression);
        }

        public bool ValidateExpression(string expression)
        {
            return true;
        }
        #endregion
    }
}
