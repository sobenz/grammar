using System;

namespace TargetingTestApp.Consumer
{
    public class ConsumerRecord
    {
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string[] Tags { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }
}
