using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TargetingTestApp.Criterion;
using System.Reflection;
using TargetingTestApp.Evaluation;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Grammar for generic top level targeting expressions. Only simple logic expressions are allowed at this level. More complex comparison operations
    /// should be defined entirely in rules.
    /// </summary>
    internal class TargetingExpressionGrammar : SimpleExpressionGrammar
    {
        private readonly IEnumerable<ICriterion> _criteria;
        private readonly IDictionary<Type, IRuleExpressionParser> _ruleParsers;

        private static readonly Dictionary<string, MethodInfo> _evaluatorMethods =
            typeof(ITargetEvaluator).GetMethods(BindingFlags.Instance | BindingFlags.Public).ToDictionary(m => m.Name, m => m);
        private static readonly ParameterExpression _evaluator = Expression.Parameter(typeof(ITargetEvaluator), "evaluator");

        public TargetingExpressionGrammar(ILogger<TargetingExpressionGrammar> logger, IEnumerable<ICriterion> criteria, IEnumerable<IRuleExpressionParser> ruleParsers) : base(logger)
        {
            _criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
            if (ruleParsers == null)
                throw new ArgumentNullException(nameof(ruleParsers));
            _ruleParsers = ruleParsers.ToDictionary(p => p.RuleParameterType);
        }

        protected internal virtual Parser<Expression> TargetEvaluation => from targetRef in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                                                         select BuildEvaluationForTargetReference(targetRef);
        protected internal override Parser<Expression> ExpressionComponent => base.ExpressionComponent.Or(TargetEvaluation);

        protected internal override Parser<Expression> Lambda => BinaryOperation.End().Select(body => Expression.Lambda<Func<ITargetEvaluator, bool>>(body, _evaluator));

        private Expression BuildEvaluationForTargetReference(string referenceCode)
        {
            var criterion = _criteria.FirstOrDefault(c => c.ReferenceCode.Equals(referenceCode, StringComparison.OrdinalIgnoreCase));

            if (criterion == null)
            {
                throw new TargetExpressionException(referenceCode, "Reference code does not match to a valid criterion.");
            }
            return criterion.CriteriaType switch
            {
                CriterionType.Segment => BinaryOperation.Parse(((Segment)criterion).SegmentExpression),
                CriterionType.Simple => Expression.Call(_evaluator, _evaluatorMethods[nameof(ITargetEvaluator.HasTag)], Expression.Constant(referenceCode)),
                CriterionType.Rule => BuildRuleExpression((Rule)criterion),
                _ => throw new TargetExpressionException(referenceCode, $"Criterion type '{criterion.CriteriaType}' is not supported by this parser."),
            };
        }

        private Expression BuildRuleExpression(Rule rule)
        {
            var val = Expression.TypeAs(Expression.Call(_evaluator, _evaluatorMethods[nameof(ITargetEvaluator.GetRuleTarget)], Expression.Constant(rule.EvaluationTarget)), rule.EvaluationType);

            _ruleParsers.TryGetValue(rule.EvaluationType, out IRuleExpressionParser ruleParser);
            if (ruleParser == null)
                throw new TargetExpressionException(rule.ReferenceCode, $"Unsupported Rule type: {rule.EvaluationType.Name}");

            Expression ruleLambda = ruleParser.GenerateExpression(rule.EvaluationCriterion);
            return Expression.Invoke(ruleLambda, val);
        }
    }
}
