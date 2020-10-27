using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Linq.Expressions;

namespace TargetingTestApp.Grammar
{
    abstract class ParametizedExpressionGrammar<TParameterType, TExtensionOperators, TExtensionFunctions> : LogicalExpressionGrammar<TExtensionOperators, TExtensionFunctions>
        where TExtensionOperators : OperatorExtensions
        where TExtensionFunctions : FunctionExtensions
    {
        public ParametizedExpressionGrammar(ILogger<ParametizedExpressionGrammar<TParameterType, TExtensionOperators, TExtensionFunctions>> logger) : base(logger)
        {
        }

        private static readonly ParameterExpression ParamExpression = Expression.Parameter(typeof(TParameterType), "x");
        protected internal virtual Parser<Expression> Parameter => Parse.String("x").Token().Return(ParamExpression);

        protected internal override Parser<Expression> Operand => base.Operand.Or(Parameter);
        protected internal override Parser<Expression> Lambda => BinaryOperation.End().Select(body => Expression.Lambda<Func<TParameterType, bool>>(body, ParamExpression));
    }
}
