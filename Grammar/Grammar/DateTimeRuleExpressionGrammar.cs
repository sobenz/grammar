using Microsoft.Extensions.Logging;
using System;

namespace TargetingTestApp.Grammar
{
    internal class DateTimeRuleExpressionGrammar : ParametizedExpressionGrammar<DateTime, OperatorExtensions.None, DateTimeRuleExpressionGrammar.DateTimeFunctions>, IRuleExpressionParser
    {
        public DateTimeRuleExpressionGrammar(ILogger<DateTimeRuleExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(DateTime);

        public class DateTimeFunctions : FunctionExtensions
        {
            public static string MonthOf(DateTime eventDate)
            {
                return eventDate.Month switch
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

            public static DateTime Now()
            {
                return DateTime.Now;
            }

            public static double DayRangeFromDate(DateTime eventDate, bool forwardsOnly)
            {
                var eventDay = eventDate.DayOfYear;
                var currentDay = DateTime.Now.DayOfYear;

                if (forwardsOnly)
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
    }
}
