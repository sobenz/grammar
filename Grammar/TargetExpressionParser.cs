using Sprache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using TargetingTestApp.Criterion;
using TargetingTestApp.Evaluation;
using TargetingTestApp.Grammar;

namespace TargetingTestApp
{
    internal class TargetExpressionParser : ITargetExpressionParser
    {
        private readonly TargetingExpressionGrammar _grammar;

        //Evaluator Definitions
        private static readonly Dictionary<string, MethodInfo> _evaluatorMethods =
            typeof(ITargetEvaluator).GetMethods(BindingFlags.Instance | BindingFlags.Public).ToDictionary(m => m.Name, m => m);
        private static readonly ParameterExpression _evaluator = Expression.Parameter(typeof(ITargetEvaluator), "evaluator");

        //Grammar Constructs
        private static readonly Parser<ExpressionType> _and = Parse.String("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
        private static readonly Parser<ExpressionType> _or = Parse.String("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
        private static readonly Parser<ExpressionType> _binaryOperator = _and.Or(_or);
        private readonly Parser<Expression> _targetEvaluation;
        private readonly Parser<Expression> _term;
        private readonly Parser<Expression> _negationOperation;
        private readonly Parser<Expression> _binaryOperation;
        private readonly Parser<Expression<Func<ITargetEvaluator,bool>>> _lambda;

        //Member Variables
        private readonly IEnumerable<ICriterion> _criteria;

        public TargetExpressionParser(IEnumerable<ICriterion> criteria, TargetingExpressionGrammar grammar)
        {
            _grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
            #region Grammar
            _targetEvaluation = from targetRef in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                select BuildEvaluationForTargetReference(targetRef);

            _term = (from lparen in Parse.Char('(')
                    from expression in Parse.Ref(() => _binaryOperation)
                    from rparen in Parse.Char(')')
                    select expression).Named("expression")
                    .XOr(_targetEvaluation);

            _negationOperation = (from negation in Parse.String("NOT").Or(Parse.String("!")).Token()
                                 from term in _term
                                 select Expression.Not(term)).Token();
          
            _binaryOperation = Parse.ChainOperator(_binaryOperator, _negationOperation.Or(_term), Expression.MakeBinary);

            _lambda = _binaryOperation.End().Select(body => Expression.Lambda<Func<ITargetEvaluator, bool>>(body, _evaluator));
            #endregion

            _criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        #region ITargetExpressionParser
        public Expression<Func<ITargetEvaluator, bool>> GenerateExpression(string expression)
        {
            return (Expression<Func<ITargetEvaluator, bool>>)_grammar.GenerateExpression(expression);
            //return _lambda.Parse(expression);
        }

        public bool ValidateExpression(string expression)
        {
            return true;
        }
        #endregion

        private Expression BuildEvaluationForTargetReference(string referenceCode)
        {
            var criterion = _criteria.FirstOrDefault(c => c.ReferenceCode.Equals(referenceCode, StringComparison.OrdinalIgnoreCase));

            if (criterion == null)
            {
                throw new TargetExpressionException(referenceCode, "Reference code does not match to a valid criterion.");
            }
            switch(criterion.CriteriaType)
            {
                case CriterionType.Segment:
                    return _binaryOperation.Parse(((Segment)criterion).SegmentExpression);
                case CriterionType.Simple:
                    return Expression.Call(_evaluator, _evaluatorMethods[nameof(ITargetEvaluator.HasTag)], Expression.Constant(referenceCode));
                case CriterionType.Rule:
                    return BuildRuleExpression((Rule)criterion);
                default:
                    throw new TargetExpressionException(referenceCode, $"Criterion type '{criterion.CriteriaType}' is not supported by this parser.");
            }          
        }

        private Expression BuildRuleExpression(Rule rule)
        {
            var val = Expression.Convert(Expression.Call(_evaluator, _evaluatorMethods[nameof(ITargetEvaluator.GetRuleTarget)], Expression.Constant(rule.EvaluationTarget)), rule.EvaluationType);
            Expression ruleLambda = null;
            switch(rule.EvaluationType.Name)
            {
                case nameof(Int32):
                    ruleLambda = NumericExpressionGrammar<int>.GenerateBodyExpression(rule.EvaluationCriterion);
                    break;
                case nameof(Double):
                    ruleLambda = NumericExpressionGrammar<double>.GenerateBodyExpression(rule.EvaluationCriterion);
                    break;
                case nameof(String):
                    ruleLambda = StringExpressionGrammar.GenerateBodyExpression(rule.EvaluationCriterion);
                    break;
                case nameof(DateTime):
                    ruleLambda = DateTimeExpressionGrammar.GenerateBodyExpression(rule.EvaluationCriterion);
                    break;
                default:
                    throw new TargetExpressionException(rule.ReferenceCode, $"Unsupported Rule type: {rule.EvaluationType.Name}");
            }
            return Expression.Invoke(ruleLambda, val);
        }

        private class StringExpressionGrammar
        {
            private static readonly Parser<ExpressionType> _equals = Parse.String("==").Or(Parse.String("=")).Token().Return(ExpressionType.Equal);
            private static readonly Parser<ExpressionType> _notEquals = Parse.String("<>").Or(Parse.String("!=")).Token().Return(ExpressionType.NotEqual);
            private static readonly Parser<ExpressionType> _and = Parse.String("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
            private static readonly Parser<ExpressionType> _or = Parse.String("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
            private static readonly Parser<ExpressionType> _binaryOperator = _and.Or(_or).Or(_equals).Or(_notEquals);

            private static readonly Parser<string> _builtInOperationNames = Parse.IgnoreCase(nameof(StringOperations.Like)).Text().Token()
                                                                            .Or(Parse.IgnoreCase(nameof(StringOperations.StartsWith)).Text().Token())
                                                                            .Or(Parse.IgnoreCase(nameof(StringOperations.EndsWith)).Text().Token())
                                                                            .Or(Parse.IgnoreCase(nameof(StringOperations.RegexMatches)).Text().Token());

            private static readonly Parser<Expression> _builtInOperations = from lhs in Parse.Ref(() => _term)
                                                                            from func in _builtInOperationNames
                                                                            from rhs in Parse.Ref(() => _term)
                                                                            select BuiltInFunctions(func, lhs, rhs);

            private static readonly Parser<Char> _escapeChar = from marker in Parse.Char('\\')
                                                               from escapedChar in Parse.AnyChar
                                                               select escapedChar;
            private static readonly Parser<Expression> _constant = from lquot in Parse.Char('"')
                                                                   from val in _escapeChar.XOr( Parse.CharExcept('"')).Many().Text()
                                                                   from rquot in Parse.Char('"').Token()
                                                                   select Expression.Constant(val);

            private static readonly ParameterExpression _paramExpression = Expression.Parameter(typeof(string), "x");
            private static readonly Parser<Expression> _parameter = Parse.String("x").Token().Return(_paramExpression);

            private static readonly Parser<Expression> _term = (from lparen in Parse.Char('(')
                                                                from expression in Parse.Ref(() => _binaryOperation)
                                                                from rparen in Parse.Char(')')
                                                                select expression).Named("expression")
                                                                .XOr(_constant).XOr(_parameter);

            private static readonly Parser<Expression> _negationOperation = (from negation in Parse.String("NOT").Or(Parse.String("!")).Token()
                                                                             from term in _term
                                                                             select Expression.Not(term)).Token();

            private static readonly Parser<Expression> _binaryOperation = _builtInOperations.Or(Parse.ChainOperator(_binaryOperator, _negationOperation.XOr(_term), Expression.MakeBinary));

            private static readonly Parser<Expression> _lambda = _binaryOperation.End().Select(body => Expression.Lambda<Func<string, bool>>(body, _paramExpression));


            private static Expression BuiltInFunctions(string name, Expression lhs, Expression rhs)
            {
                return Expression.Call(
                    typeof(StringOperations).GetMethod(name,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase)
                        , lhs, rhs);
            }

            public static Expression GenerateBodyExpression(string expression)
            {
                //TODO Add better error handling.
                return _lambda.Parse(expression);
            }

            private static class StringOperations
            {
                private static readonly ConcurrentDictionary<string, Regex> _regexMatches = new ConcurrentDictionary<string, Regex>();

                public static bool Like(string source, string match)
                {
                    return source.Contains(match);
                }

                public static bool StartsWith(string source, string start)
                {
                    return source.StartsWith(start);
                }

                public static bool EndsWith(string source, string end)
                {
                    return source.EndsWith(end);
                }

                public static bool RegexMatches(string source, string pattern)
                {
                    var regex = _regexMatches.GetOrAdd(pattern, p => new Regex(pattern, RegexOptions.Compiled));
                    return regex.IsMatch(source);
                }
            }
        }

        private class NumericExpressionGrammar<T>
        {
            private static readonly Parser<ExpressionType> _equals = Parse.String("==").Or(Parse.String("=")).Token().Return(ExpressionType.Equal);
            private static readonly Parser<ExpressionType> _notEquals = Parse.String("<>").Or(Parse.String("!=")).Token().Return(ExpressionType.NotEqual);
            private static readonly Parser<ExpressionType> _lessThan = Parse.String("<").Token().Return(ExpressionType.LessThan);
            private static readonly Parser<ExpressionType> _lessThanOrEqual = Parse.String("<=").Token().Return(ExpressionType.LessThanOrEqual);
            private static readonly Parser<ExpressionType> _greaterThan = Parse.String(">").Token().Return(ExpressionType.GreaterThan);
            private static readonly Parser<ExpressionType> _greaterThanOrEqual = Parse.String(">=").Token().Return(ExpressionType.GreaterThanOrEqual);
            private static readonly Parser<ExpressionType> _and = Parse.String("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
            private static readonly Parser<ExpressionType> _or = Parse.String("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
            private static readonly Parser<ExpressionType> _binaryOperator = _and.Or(_or).Or(_equals).Or(_notEquals).Or(_lessThanOrEqual).Or(_lessThan).Or(_greaterThanOrEqual).Or(_greaterThan);

            private static readonly Parser<Expression> _constant = from val in Parse.Number.Token()
                                                                   select Expression.Constant(int.Parse(val));

            private static readonly ParameterExpression _paramExpression = Expression.Parameter(typeof(T), "x");
            private static readonly Parser<Expression> _parameter = Parse.String("x").Token().Return(_paramExpression);

            private static readonly Parser<Expression> _term = (from lparen in Parse.Char('(')
                                                                from expression in Parse.Ref(() => _binaryOperation)
                                                                from rparen in Parse.Char(')')
                                                                select expression).Named("expression")
                                                                .XOr(_constant).XOr(_parameter);

            private static readonly Parser<Expression> _negationOperation = (from negation in Parse.String("NOT").Or(Parse.String("!")).Token()
                                                                             from term in _term
                                                                             select Expression.Not(term)).Token();

            private static readonly Parser<Expression> _binaryOperation = Parse.ChainOperator(_binaryOperator, _negationOperation.XOr(_term), Expression.MakeBinary);

            private static readonly Parser<Expression> _lambda = _binaryOperation.End().Select(body => Expression.Lambda<Func<T, bool>>(body, _paramExpression));

            public static Expression GenerateBodyExpression(string expression)
            {
                //TODO Add better error handling.
                return _lambda.Parse(expression);
            }
        }

        private class DateTimeExpressionGrammar
        {
            private static readonly Parser<ExpressionType> _equals = Parse.String("==").Or(Parse.String("=")).Token().Return(ExpressionType.Equal);
            private static readonly Parser<ExpressionType> _notEquals = Parse.String("<>").Or(Parse.String("!=")).Token().Return(ExpressionType.NotEqual);
            private static readonly Parser<ExpressionType> _lessThan = Parse.String("<").Token().Return(ExpressionType.LessThan);
            private static readonly Parser<ExpressionType> _lessThanOrEqual = Parse.String("<=").Token().Return(ExpressionType.LessThanOrEqual);
            private static readonly Parser<ExpressionType> _greaterThan = Parse.String(">").Token().Return(ExpressionType.GreaterThan);
            private static readonly Parser<ExpressionType> _greaterThanOrEqual = Parse.String(">=").Token().Return(ExpressionType.GreaterThanOrEqual);
            private static readonly Parser<ExpressionType> _and = Parse.String("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
            private static readonly Parser<ExpressionType> _or = Parse.String("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
            private static readonly Parser<ExpressionType> _binaryOperator = _and.Or(_or).Or(_equals).Or(_notEquals).Or(_lessThanOrEqual).Or(_lessThan).Or(_greaterThanOrEqual).Or(_greaterThan);

            private static readonly Parser<string> _builtInFunctionNames = Parse.IgnoreCase(nameof(DateTimeFunctions.MonthOf)).Text()
                                                                           .Or(Parse.IgnoreCase(nameof(DateTimeFunctions.Now)).Text())
                                                                           .Or(Parse.IgnoreCase(nameof(DateTimeFunctions.DayRangeFromDate)).Text());

            private static readonly Parser<Expression> _builtInFunctions = from func in _builtInFunctionNames
                                                                           from lparen in Parse.Char('(')
                                                                           from funcParams in (from t in Parse.Ref(() => _term) from comma in Parse.Char(',').Token().Optional() select t).Many()
                                                                           from rparen in Parse.Char(')').Token()
                                                                           select BuiltInFunctions(func, funcParams.ToArray());

            private static readonly Parser<Expression> _numericConstant = from val in Parse.Number.Token()
                                                                         select Expression.Constant(int.Parse(val));
            private static readonly Parser<Expression> _boolConstant = from val in Parse.IgnoreCase("true").Or(Parse.IgnoreCase("false")).Text().Token()
                                                                       select Expression.Constant(bool.Parse(val));
            private static readonly Parser<Char> _escapeChar = from marker in Parse.Char('\\')
                                                               from escapedChar in Parse.AnyChar
                                                               select escapedChar;
            private static readonly Parser<Expression> _stringConstant = from lquot in Parse.Char('"')
                                                                         from val in _escapeChar.XOr(Parse.CharExcept('"')).Many().Text()
                                                                         from rquot in Parse.Char('"').Token()
                                                                         select Expression.Constant(val);
            private static readonly Parser<Expression> _constant = _numericConstant.XOr(_stringConstant).XOr(_boolConstant);

            private static readonly ParameterExpression _paramExpression = Expression.Parameter(typeof(DateTime), "x");
            private static readonly Parser<Expression> _parameter = Parse.String("x").Token().Return(_paramExpression);

            private static readonly Parser<Expression> _term = (from lparen in Parse.Char('(')
                                                                from expression in Parse.Ref(() => _binaryOperation)
                                                                from rparen in Parse.Char(')')
                                                                select expression).Named("expression")
                                                                .XOr(_builtInFunctions).XOr(_constant).XOr(_parameter);

            private static readonly Parser<Expression> _negationOperation = (from negation in Parse.String("NOT").Or(Parse.String("!")).Token()
                                                                             from term in _term
                                                                             select Expression.Not(term)).Token();

            private static readonly Parser<Expression> _binaryOperation = Parse.ChainOperator(_binaryOperator, _negationOperation.XOr(_term), Expression.MakeBinary);

            private static readonly Parser<Expression> _lambda = _binaryOperation.End().Select(body => Expression.Lambda<Func<DateTime, bool>>(body, _paramExpression));

            public static Expression GenerateBodyExpression(string expression)
            {
                //TODO Add better error handling.
                return _lambda.Parse(expression);
            }

            private static Expression BuiltInFunctions(string name, params Expression[] parameters)
            {
                return Expression.Call(typeof(DateTimeFunctions).GetMethod(name,
                                        BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase)
                                        ,parameters);
            }

            private static class DateTimeFunctions
            {
                public static string MonthOf(DateTime eventDate)
                {
                    switch(eventDate.Month)
                    {
                        case 1: return "January";
                        case 2: return "February";
                        case 3: return "March";
                        case 4: return "April";
                        case 5: return "May";
                        case 6: return "June";
                        case 7: return "July";
                        case 8: return "August";
                        case 9: return "September";
                        case 10: return "October";
                        case 11: return "November";
                        case 12: return "December";
                        default: throw new ArgumentException("Invalid DateTime", nameof(eventDate));
                    }
                }

                public static DateTime Now()
                {
                    return DateTime.Now;
                }

                public static int DayRangeFromDate(DateTime eventDate, bool forwardsOnly)
                {
                    var eventDay = eventDate.DayOfYear;
                    var currentDay = DateTime.Now.DayOfYear;

                    if (forwardsOnly)
                    {
                        if (currentDay < eventDay)
                            currentDay += 365;
                        return currentDay-eventDay;
                    }
                    else
                    {
                        if (currentDay > eventDay)
                            eventDay += 365;
                        return eventDay - currentDay;
                    }
                }
            }
        }
    }
}
