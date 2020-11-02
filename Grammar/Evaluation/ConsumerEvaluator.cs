using System;
using System.Collections.Generic;
using System.Linq;
using TargetingTestApp.Consumer;
using TargetingTestApp.Grammar;

namespace TargetingTestApp.Evaluation
{
    public class ConsumerEvaluator : ITargetEvaluator
    {
        private readonly ConsumerRecord _consumer;

        public ConsumerEvaluator(ConsumerRecord consumer)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        }

        public bool HasTag(string referenceCode)
        {
            Console.WriteLine($"Test:{referenceCode}");
            return _consumer.Tags.Any(t => t.Equals(referenceCode, StringComparison.OrdinalIgnoreCase));
        }

        public object GetRuleTarget(string target)
        {
            IEnumerable<ConsumerEvent> events = null;
            switch (target)
            {
                case "Age":
                    double? age = _consumer.DateOfBirth.HasValue ? (double?)Math.Floor(DateTime.Now.Subtract(_consumer.DateOfBirth.Value).TotalDays / 365) : null;
                    Console.WriteLine($"Extracting age:{age}");
                    return age;
                case "DateOfBirth":
                    Console.WriteLine($"Extracting DateOFBirth:{_consumer.DateOfBirth}");
                    return _consumer.DateOfBirth;
                case "Name":
                    Console.WriteLine($"Extracting name:{_consumer.Name}");
                    return _consumer.Name;
                case "CurrentMarket":
                    Console.WriteLine($"Extracting market:{_consumer.CurrentMarket}");
                    return _consumer.CurrentMarket;
                case "Gender":
                    Console.WriteLine($"Extracting gender:{_consumer.Gender}");
                    return _consumer.Gender.ToString();
                case "RegistrationDate":
                    Console.WriteLine($"Extracting name:{_consumer.RegistrationDate}");
                    return _consumer.RegistrationDate;
                case "Redemptions":
                    events = _consumer.ConsumerEvents.Where(e => e.EventType == ConsumerEventType.Redemption);
                    Console.WriteLine($"Extracting Redemption events:{events.Count()}");
                    return events;
                case "AppStarts":
                    events = _consumer.ConsumerEvents.Where(e => e.EventType == ConsumerEventType.AppStart);
                    Console.WriteLine($"Extracting AppStart events:{events.Count()}");
                    return events;
                case "PointSpend":
                    events = _consumer.ConsumerEvents.Where(e => e.EventType == ConsumerEventType.PointsSpend);
                    Console.WriteLine($"Extracting PointsSpend events:{events.Count()}");
                    return events;
                case "ProductPurchase":
                    events = _consumer.ConsumerEvents.Where(e => e.EventType == ConsumerEventType.ProductPurchase);
                    Console.WriteLine($"Extracting ProductPurchase events:{events.Count()}");
                    return events;
                case "Rewards":
                    events = _consumer.ConsumerEvents.Where(e => e.EventType == ConsumerEventType.RewardActivation);
                    Console.WriteLine($"Extracting RewardActivation events:{events.Count()}");
                    return events;
                default:
                    throw new TargetExpressionException("target", "Invalid Rule");
            }
        }
    }
}
