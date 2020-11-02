using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Extends the grammar defined in <see cref="ComparativeExpressionGrammar"/> by adding support for custom operators and custom functions to the grammar.
    /// Custom functions and operators take their name from the method that defines their behaviour.
    /// </summary>
    /// <typeparam name="TExtensionOperators">The class that defines the static methods for the extended operators.</typeparam>
    /// <typeparam name="TExtensionFunctions">The class that defines the static methods for the extended functions.</typeparam>
    abstract class ExtensibleExpressionGrammar<TExtensionOperators,TExtensionFunctions> : ComparativeExpressionGrammar
        where TExtensionOperators : OperatorExtensions 
        where TExtensionFunctions : FunctionExtensions
    {
        public ExtensibleExpressionGrammar(ILogger<ExtensibleExpressionGrammar<TExtensionOperators, TExtensionFunctions>> logger) : base(logger)
        {
        }

        #region Extended Operators
        protected internal virtual IDictionary<string,MethodInfo> ExtensionOperatorMethods => typeof(TExtensionOperators).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                              .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        protected internal virtual Parser<MethodInfo> ExtensionOperatorInfo => from extensionName in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                                                               where ExtensionOperatorMethods.ContainsKey(extensionName)
                                                                               select ExtensionOperatorMethods[extensionName];
        protected internal virtual Parser<Expression> ExtensionOperator => from lhs in Parse.Ref(() => ExpressionComponent)
                                                                           from func in ExtensionOperatorInfo
                                                                           from rhs in Parse.Ref(() => ExpressionComponent)
                                                                           select Expression.Call(func, lhs, rhs);
        #endregion

        #region Extended Functions
        protected internal virtual IDictionary<string,MethodInfo> ExtensionFunctionMethods => typeof(TExtensionFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                              .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        protected internal virtual Parser<MethodInfo> ExtensionFunctionInfo => from extensionName in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                                                               where ExtensionFunctionMethods.ContainsKey(extensionName)
                                                                               select ExtensionFunctionMethods[extensionName];

        protected internal virtual Parser<Expression> ExtensionFunction => from func in ExtensionFunctionInfo
                                                                           from lparen in Parse.Char('(')
                                                                           from funcParams in Parse.DelimitedBy(Parse.Ref(() => ExpressionComponent), Parse.Char(',').Token()).Optional()
                                                                           from rparen in Parse.Char(')').Token()
                                                                           select Expression.Call(func, funcParams.GetOrDefault());
        #endregion

        #region Operations
        protected internal override Parser<Expression> BinaryOperation => Parse.ChainOperator(BinaryOperators, NegationOperation.Or(ExtensionOperator).Or(ExpressionComponent), Expression.MakeBinary);
        #endregion

        #region Structure
        protected internal override Parser<Expression> ExpressionComponent => base.ExpressionComponent.Or(ExtensionFunction);
        #endregion
    }
}
