using System;

namespace TargetingTestApp.Consumer
{
    public class ConsumerRecord
    {
        public string Name { get; set; }
        public string CurrentMarket { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string[] Tags { get; set; }

        public ConsumerEvent[] ConsumerEvents { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }
}
