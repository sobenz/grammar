using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Grammar for rules that are based on string properties.
    /// </summary>
    internal class StringRuleExpressionGrammar : ParametizedExpressionGrammar<string, StringRuleExpressionGrammar.StringOperations, FunctionExtensions.None>, IRuleExpressionParser
    {
        public StringRuleExpressionGrammar(ILogger<StringRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(string);

        public class StringOperations : OperatorExtensions
        {
            private static readonly ConcurrentDictionary<string, Regex> _regexMatches = new ConcurrentDictionary<string, Regex>();

            /// <summary>
            /// Determines if a string is contained within another string.
            /// </summary>
            /// <param name="source">The containing string.</param>
            /// <param name="match">The string to look for.</param>
            /// <returns>True if the string is found otherwise false.</returns>
            public static bool Contains(string source, string match)
            {
                return source.Contains(match);
            }

            /// <summary>
            /// Determines if a string starts with another string.
            /// </summary>
            /// <param name="source">The containing string.</param>
            /// <param name="start">The string to look for.</param>
            /// <returns>True if the string is found otherwise false.</returns>
            public static bool StartsWith(string source, string start)
            {
                return source.StartsWith(start);
            }

            /// <summary>
            /// Determines if a string ends with another string.
            /// </summary>
            /// <param name="source">The containing string.</param>
            /// <param name="end">The string to look for.</param>
            /// <returns>True if the string is found otherwise false.</returns>
            public static bool EndsWith(string source, string end)
            {
                return source.EndsWith(end);
            }

            /// <summary>
            /// Determines if a string matches with a given regex string.
            /// </summary>
            /// <param name="source">The containing string.</param>
            /// <param name="pattern">The string to look for.</param>
            /// <returns>True if the regex pattern mateches the provided string, otherwise false.</returns>
            public static bool RegexMatches(string source, string pattern)
            {
                var regex = _regexMatches.GetOrAdd(pattern, p => new Regex(pattern, RegexOptions.Compiled));
                return regex.IsMatch(source);
            }
        }
    }
}
