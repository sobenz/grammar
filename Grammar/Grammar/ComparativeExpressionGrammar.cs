using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Linq.Expressions;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Extends the <see cref="SimpleExpressionGrammar"/> adding additional support for operators - <=, <, >=, > and also the ability to
    /// set constant values as part of the expression. Constants can be in the form of bool, double, and string,
    /// </summary>
    abstract class ComparativeExpressionGrammar : SimpleExpressionGrammar
    {
        public ComparativeExpressionGrammar(ILogger<ComparativeExpressionGrammar> logger) : base(logger)
        {
        }

        #region Operators
        protected internal virtual Parser<ExpressionType> OpLessThan => Parse.String("<").Token().Return(ExpressionType.LessThan);
        protected internal virtual Parser<ExpressionType> OpLessThanOrEqual => Parse.String("<=").Token().Return(ExpressionType.LessThanOrEqual);
        protected internal virtual Parser<ExpressionType> OpGreaterThan => Parse.String(">").Token().Return(ExpressionType.GreaterThan);
        protected internal virtual Parser<ExpressionType> OpGreaterThanOrEqual => Parse.String(">=").Token().Return(ExpressionType.GreaterThanOrEqual);
        protected internal override Parser<ExpressionType> BinaryOperators => base.BinaryOperators.Or(OpLessThanOrEqual).Or(OpLessThan)
                                                                                 .Or(OpGreaterThanOrEqual).Or(OpGreaterThan);
        #endregion

        #region Constants
        protected internal virtual Parser<Expression> BoolConstant => from boolConst in Parse.IgnoreCase("true").Or(Parse.IgnoreCase("false")).Text().Token()
                                                                      select Expression.Constant(bool.Parse(boolConst));

        //All constants returned as nullable types as specific rules may return nullable values as well, and this prevents constant conversion when 
        //performing comparison operations.
        protected internal virtual Parser<Expression> NumericConstant => from numericConst in Parse.Regex("-?[0-9]+(.[0-9]+)?", "Numeric pattern").Token()
                                                                         select Expression.Constant(double.Parse(numericConst), typeof(double?));
        protected internal virtual Parser<Char> EscapeChar => from marker in Parse.Char('\\')
                                                              from escapedChar in Parse.AnyChar
                                                              select escapedChar;
        protected internal virtual Parser<Expression> StringConstant => from lquot in Parse.Char('"')
                                                                        from stringConstant in EscapeChar.XOr(Parse.CharExcept('"')).Many().Text()
                                                                        from rquot in Parse.Char('"').Token()
                                                                        select Expression.Constant(stringConstant);

        protected internal virtual Parser<Expression> Constant => BoolConstant.Or(NumericConstant).Or(StringConstant);
        #endregion

        #region Structure
        protected internal override Parser<Expression> ExpressionComponent => base.ExpressionComponent.Or(Constant);
        #endregion
    }
}
