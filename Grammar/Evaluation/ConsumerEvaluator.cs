using System;
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
                default:
                    throw new TargetExpressionException("target", "Invalid Rule");
            }
        }
    }
}
