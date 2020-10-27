using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace TargetingTestApp.Grammar
{
    internal class StringRuleExpressionGrammar : ParametizedExpressionGrammar<string, StringRuleExpressionGrammar.StringOperations, FunctionExtensions.None>, IRuleExpressionParser
    {
        public StringRuleExpressionGrammar(ILogger<StringRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(string);

        public class StringOperations : OperatorExtensions
        {
            private static readonly ConcurrentDictionary<string, Regex> _regexMatches = new ConcurrentDictionary<string, Regex>();

            public static bool Contains(string source, string match)
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
}
