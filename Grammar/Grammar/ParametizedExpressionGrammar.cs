using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Linq.Expressions;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Extends the functionality of the grammar defined in <see cref="ExtensibleExpressionGrammar{TExtensionOperators, TExtensionFunctions}"/> by allowing
    /// parametized data to be referenced in the grammar. TYpically this will be used by rules that will reference specific data from the entity that is being 
    /// evaluated.
    /// </summary>
    /// <typeparam name="TParameterType">The Type of data thats provided in the parameter.</typeparam>
    /// <typeparam name="TExtensionOperators">The class that defines the static methods for the extended operators.</typeparam>
    /// <typeparam name="TExtensionFunctions">The class that defines the static methods for the extended functions.</typeparam>
    abstract class ParametizedExpressionGrammar<TParameterType, TExtensionOperators, TExtensionFunctions> : ExtensibleExpressionGrammar<TExtensionOperators, TExtensionFunctions>
        where TExtensionOperators : OperatorExtensions
        where TExtensionFunctions : FunctionExtensions
    {
        public ParametizedExpressionGrammar(ILogger<ParametizedExpressionGrammar<TParameterType, TExtensionOperators, TExtensionFunctions>> logger) : base(logger)
        {
        }

        #region Paramatization
        private static readonly ParameterExpression ParamExpression = Expression.Parameter(typeof(TParameterType), "x");
        protected internal virtual Parser<Expression> Parameter => Parse.String("x").Token().Return(ParamExpression);
        #endregion

        #region Structure
        protected internal override Parser<Expression> ExpressionComponent => base.ExpressionComponent.Or(Parameter);
        protected internal override Parser<Expression> Lambda => BinaryOperation.End().Select(body => Expression.Lambda<Func<TParameterType, bool>>(body, ParamExpression));
        #endregion
    }
}
