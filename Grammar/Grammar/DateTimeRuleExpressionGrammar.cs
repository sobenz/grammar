using Microsoft.Extensions.Logging;
using System;

namespace TargetingTestApp.Grammar
{
    /// <summary>
    /// Grammar for rules that are based on DateTime properties.  Additional functions for processing date time data are also defined.
    /// </summary>
    /// <remarks>All numeric values are treated as nullable date times.</remarks>
    internal class DateTimeRuleExpressionGrammar : ParametizedExpressionGrammar<DateTime?, OperatorExtensions.None, DateTimeRuleExpressionGrammar.DateTimeFunctions>, IRuleExpressionParser
    {
        public DateTimeRuleExpressionGrammar(ILogger<DateTimeRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(DateTime?);

        #region Extended Functions
        /// <summary>
        /// Datetimes that are returned or accepted should always be considered as DateTime? and the null case scenario should be explicitly handled.
        /// </summary>
        public class DateTimeFunctions : FunctionExtensions
        {
            /// <summary>
            /// Gets the textual representation of the month of a specific DateTime
            /// </summary>
            /// <param name="eventDate">The datetime to analyse.</param>
            /// <returns>The name of the month that the event occurred on.</returns>
            public static string MonthOf(DateTime? eventDate)
            {
                if (!eventDate.HasValue)
                    return string.Empty;

                return eventDate.Value.Month switch
                {
                    1 => "January",
                    2 => "February",
                    3 => "March",
                    4 => "April",
                    5 => "May",
                    6 => "June",
                    7 => "July",
                    8 => "August",
                    9 => "September",
                    10 => "October",
                    11 => "November",
                    12 => "December",
                    _ => throw new ArgumentException("Invalid DateTime", nameof(eventDate)),
                };
            }

            /// <summary>
            /// Gets the current UTC DateTime.
            /// </summary>
            /// <returns>The current UTC date time.</returns>
            public static DateTime? Now()
            {
                return DateTime.UtcNow;
            }

            /// <summary>
            /// Returns the number of days from the current UTC date to a specific date time.
            /// </summary>
            /// <param name="eventDate">The date time to compare to now.</param>
            /// <param name="lookForwards"> Whether to looks forwards or backwards.</param>
            /// <returns>The number of days between the two days.</returns>
            public static double? DayRangeFromDate(DateTime? eventDate, bool lookForwards)
            {
                //TODO - Handle leap years.
                if (!eventDate.HasValue)
                    return double.NaN;

                var eventDay = eventDate.Value.DayOfYear;
                var currentDay = DateTime.Now.DayOfYear;

                if (lookForwards)
                {
                    if (currentDay < eventDay)
                        currentDay += 365;
                    return currentDay - eventDay;
                }
                else
                {
                    if (currentDay > eventDay)
                        eventDay += 365;
                    return eventDay - currentDay;
                }
            }
        }
        #endregion
    }
}
