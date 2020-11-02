using System;
using TargetingTestApp.Consumer;

namespace TargetingTestApp
{
    public static class TestConsumer
    {
        public static ConsumerRecord Get()
        {
            var registrationDate = DateTime.Now.AddDays(-22);
            return new ConsumerRecord
            {
                Name = "Ben Vaughan",
                CurrentMarket = "New Zealand",
                RegistrationDate = registrationDate,
                DateOfBirth = new DateTime(1976, 12, 17),
                Gender = Gender.Male,
                Tags = new[] { "SampleGroup1", "MeatLover" },
                ConsumerEvents = new []
                {
                    new ConsumerEvent{ EventType = ConsumerEventType.AppStart, WhenOccurred = registrationDate, Market = "New Zealand" },
                    new ConsumerEvent{ EventType = ConsumerEventType.AppStart, WhenOccurred = registrationDate.AddDays(6), Market = "Australia" },
                    new ConsumerEvent{ EventType = ConsumerEventType.AppStart, WhenOccurred = registrationDate.AddDays(16), Market = "New Zealand" },
                    new ConsumerEvent{ EventType = ConsumerEventType.AppStart, WhenOccurred = registrationDate.AddDays(21), Market = "New Zealand" },
                    new ConsumerEvent{ EventType = ConsumerEventType.Redemption, WhenOccurred = registrationDate, Market = "New Zealand", Category = "Burgers", Resource = 1 },
                    new ConsumerEvent{ EventType = ConsumerEventType.Redemption, WhenOccurred = registrationDate.AddDays(21), Market = "New Zealand", Category = "Combo", Resource = 2 },
                    new ConsumerEvent{ EventType = ConsumerEventType.PointsSpend, WhenOccurred = registrationDate.AddDays(21), Market = "New Zealand", Value=100}
                }
            };
        }
    }
}
