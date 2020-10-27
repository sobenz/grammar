using Microsoft.Extensions.Logging;
using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TargetingTestApp.Grammar
{
    abstract class LogicalExpressionGrammar<TExtensionOperators,TExtensionFunctions> : IExpressionParser
        where TExtensionOperators : OperatorExtensions 
        where TExtensionFunctions : FunctionExtensions
    {
        private static readonly Type[] integralTypes = { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
        private static readonly Type[] floatingTypes = { typeof(float), typeof(double), typeof(decimal) };
        private static readonly Type[] numericTypes = integralTypes.Concat(floatingTypes).ToArray();
        protected readonly ILogger _logger;

        public LogicalExpressionGrammar(ILogger<LogicalExpressionGrammar<TExtensionOperators, TExtensionFunctions>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected internal virtual Parser<ExpressionType> OpAnd => Parse.IgnoreCase("AND").Or(Parse.String("&&")).Token().Return(ExpressionType.AndAlso);
        protected internal virtual Parser<ExpressionType> OpOr => Parse.IgnoreCase("OR").Or(Parse.String("||")).Token().Return(ExpressionType.OrElse);
        protected internal virtual Parser<ExpressionType> OpEquals => Parse.String("==").Or(Parse.String("=")).Token().Return(ExpressionType.Equal);
        protected internal virtual Parser<ExpressionType> OpNotEquals => Parse.String("<>").Or(Parse.String("!=")).Token().Return(ExpressionType.NotEqual);
        protected internal virtual Parser<ExpressionType> OpLessThan => Parse.String("<").Token().Return(ExpressionType.LessThan);
        protected internal virtual Parser<ExpressionType> OpLessThanOrEqual => Parse.String("<=").Token().Return(ExpressionType.LessThanOrEqual);
        protected internal virtual Parser<ExpressionType> OpGreaterThan => Parse.String(">").Token().Return(ExpressionType.GreaterThan);
        protected internal virtual Parser<ExpressionType> OpGreaterThanOrEqual => Parse.String(">=").Token().Return(ExpressionType.GreaterThanOrEqual);

        protected internal virtual Parser<ExpressionType> BinaryOperators => OpAnd.Or(OpOr).Or(OpEquals).Or(OpNotEquals).Or(OpLessThanOrEqual).Or(OpLessThan)
                                                                                 .Or(OpGreaterThanOrEqual).Or(OpGreaterThan);


        protected internal virtual IDictionary<string,MethodInfo> ExtensionOperatorMethods => typeof(TExtensionOperators).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                              .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        protected internal virtual Parser<MethodInfo> ExtensionOperatorInfo => from extensionName in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                                                               where ExtensionOperatorMethods.ContainsKey(extensionName)
                                                                               select ExtensionOperatorMethods[extensionName];
        protected internal virtual Parser<Expression> ExtensionOperator => from lhs in Parse.Ref(() => Operand)
                                                                           from func in ExtensionOperatorInfo
                                                                           from rhs in Parse.Ref(() => Operand)
                                                                           select Expression.Call(func, lhs, rhs);

        protected internal virtual IDictionary<string,MethodInfo> ExtensionFunctionMethods => typeof(TExtensionFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                              .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
        protected internal virtual Parser<MethodInfo> ExtensionFunctionInfo => from extensionName in Parse.LetterOrDigit.AtLeastOnce().Text().Token()
                                                                               where ExtensionFunctionMethods.ContainsKey(extensionName)
                                                                               select ExtensionFunctionMethods[extensionName];

        protected internal virtual Parser<Expression> ExtensionFunction => from func in ExtensionFunctionInfo
                                                                           from lparen in Parse.Char('(')
                                                                           from funcParams in Parse.DelimitedBy(Parse.Ref(() => Operand), Parse.Char(',').Token()).Optional()
                                                                           from rparen in Parse.Char(')').Token()
                                                                           select Expression.Call(func, funcParams.GetOrDefault());

        protected internal virtual Parser<Expression> BoolConstant => from boolConst in Parse.IgnoreCase("true").Or(Parse.IgnoreCase("false")).Text().Token()
                                                                      select Expression.Constant(bool.Parse(boolConst));
        protected internal virtual Parser<Expression> NumericConstant => from numericConst in Parse.Regex("-?[0-9]+(.[0-9]+)?", "Numeric pattern").Token()
                                                                         select Expression.Constant(double.Parse(numericConst));
        protected internal virtual Parser<Char> EscapeChar => from marker in Parse.Char('\\')
                                                              from escapedChar in Parse.AnyChar
                                                              select escapedChar;
        protected internal virtual Parser<Expression> StringConstant => from lquot in Parse.Char('"')
                                                                        from stringConstant in EscapeChar.XOr(Parse.CharExcept('"')).Many().Text()
                                                                        from rquot in Parse.Char('"').Token()
                                                                        select Expression.Constant(stringConstant);

        protected internal virtual Parser<Expression> Constant => BoolConstant.Or(NumericConstant).Or(StringConstant);

        protected internal virtual Parser<Expression> NegationOperation => (from negation in Parse.IgnoreCase("NOT").Or(Parse.String("!")).Token()
                                                                            from operand in Operand
                                                                            select Expression.Not(operand)).Token();

        protected internal virtual Parser<Expression> BinaryOperation => Parse.ChainOperator(BinaryOperators, NegationOperation.Or(ExtensionOperator).Or(Operand), Expression.MakeBinary);

        protected internal virtual Parser<Expression> Operand => (from lparen in Parse.Char('(')
                                                                  from expression in Parse.Ref(() => BinaryOperation)
                                                                  from rparen in Parse.Char(')')
                                                                  select expression).Named("expression")
                                                                  .Or(ExtensionFunction).Or(Constant);

        protected internal virtual Parser<Expression> Lambda => BinaryOperation.End().Select(body => Expression.Lambda<Func<bool>>(body));

        public Expression GenerateExpression(string text)
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
