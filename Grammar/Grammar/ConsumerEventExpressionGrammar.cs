using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TargetingTestApp.Consumer;

namespace TargetingTestApp.Grammar
{
    internal class ConsumerEventExpressionGrammar :ParametizedExpressionGrammar<IEnumerable<ConsumerEvent>, OperatorExtensions.None, ConsumerEventExpressionGrammar.ConsumerEventFunctions>, IRuleExpressionParser
    {
        public ConsumerEventExpressionGrammar(ILogger<ConsumerEventExpressionGrammar> logger) : base(logger)
        {
        }

        public Type RuleParameterType => typeof(IEnumerable<ConsumerEvent>);

        #region Extended Functions
        public class ConsumerEventFunctions : FunctionExtensions
        {
            public static double? EventsInDayRange(IEnumerable<ConsumerEvent> events, double? days)
            {
                var cutOffPoint = DateTime.UtcNow.AddDays(-days ?? 0);
                var result = events.Where(e => e.WhenOccurred > cutOffPoint).Count();
                return result;
            }

            public static double? DaysSinceLastEvent(IEnumerable<ConsumerEvent> events)
            {
                var mostRecent = events.OrderByDescending(e => e.WhenOccurred)
                                       .FirstOrDefault();
                if (mostRecent == null)
                    return double.PositiveInfinity;
                return Math.Floor(DateTime.Now.Subtract(mostRecent.WhenOccurred).TotalDays);
            }

            public static double? SumOfValueInDays(IEnumerable<ConsumerEvent> events, double? days)
            {
                var cutOffPoint = DateTime.UtcNow.AddDays(-days ?? 0);
                var eligibleEvents = events.Where(e => e.WhenOccurred > cutOffPoint);
                var result = eligibleEvents.Sum(e => e.Value ?? 0);
                return result;
            }

            public static double? EventsExceedingValue(IEnumerable<ConsumerEvent> events, double? value)
            {
                var eligibleEvents = events.Where(e => e.Value >= value);
                return eligibleEvents.Count();
            }

            public static double? EventsLessThanValue(IEnumerable<ConsumerEvent> events, double? value)
            {
                var eligibleEvents = events.Where(e => e.Value < value);
                return eligibleEvents.Count();
            }

            public static IEnumerable<ConsumerEvent> ForMarket(IEnumerable<ConsumerEvent> events, string market)
            {
                return events.Where(e => string.Equals(market, e.Market, StringComparison.OrdinalIgnoreCase));
            }
        }
        #endregion
    }
}
