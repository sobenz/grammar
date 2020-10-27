using System;
using TargetingTestApp.Consumer;

namespace TargetingTestApp
{
    public static class TestConsumer
    {
        public static ConsumerRecord Get()
        {
            return new ConsumerRecord
            {
                Name = "Ben Vaughan",
                DateOfBirth = new DateTime(1976, 12, 17),
                Gender = Gender.Male,
                Tags = new[] { "ABC", "DEF" }
            };
        }
    }
}
