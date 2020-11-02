using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Linq.Expressions;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Provides a Grammar for parsing simple logical operations allowing for simple logical operations (AND, OR, NOT, ==, !=) 
    /// and the nesting of terms in structured brackets.
    /// </summary>
    abstract class SimpleExpressionGrammar : IExpressionParser
    {
        protected readonly ILogger _logger;

        #region Operators
        protected internal virtual Parser<ExpressionType> OpAnd => Parse.IgnoreCase("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
        protected internal virtual Parser<ExpressionType> OpOr => Parse.IgnoreCase("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
        protected internal virtual Parser<ExpressionType> OpEquals => Parse.String("==").Or(Parse.String("=")).Token().Return(ExpressionType.Equal);
        protected internal virtual Parser<ExpressionType> OpNotEquals => Parse.String("<>").Or(Parse.String("!=")).Token().Return(ExpressionType.NotEqual);
        protected internal virtual Parser<ExpressionType> BinaryOperators => OpAnd.Or(OpOr).Or(OpEquals).Or(OpNotEquals);
        #endregion

        #region Operations
        protected internal virtual Parser<Expression> NegationOperation => (from negation in Parse.IgnoreCase("NOT").Or(Parse.String("!")).Token()
                                                                            from operand in ExpressionComponent
                                                                            select Expression.Not(operand)).Token();

        protected internal virtual Parser<Expression> BinaryOperation => Parse.ChainOperator(BinaryOperators, NegationOperation.Or(ExpressionComponent), Expression.MakeBinary);
        #endregion

        #region Structure
        protected internal virtual Parser<Expression> ExpressionComponent => (from lparen in Parse.Char('(')
                                                                             from expression in Parse.Ref(() => BinaryOperation)
                                                                             from rparen in Parse.Char(')')
                                                                             select expression).Named("expression");

        protected internal virtual Parser<Expression> Lambda => BinaryOperation.End().Select(body => Expression.Lambda<Func<bool>>(body));
        #endregion

        public SimpleExpressionGrammar(ILogger<SimpleExpressionGrammar> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual Expression GenerateExpression(string text)
        {
            try
            {
                return Lambda.Parse(text);
            }
            catch (ParseException pe)
            {
                _logger.LogError(pe, "Failed to parse text to valid expression. Expression Test:{text}", text);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unexpected failure occurred during parsing an expression. Expression Text:{text}", text);
                throw;
            }
        }
    }
}
