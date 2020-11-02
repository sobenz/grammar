using System;

namespace TargetingTestApp.Consumer
{
    public class ConsumerEvent
    {
        public DateTime WhenOccurred { get; set; }
        public ConsumerEventType EventType { get; set; }
        public string Market { get; set; }
        public string Category { get; set; }
        public int Resource { get; set; }
        public double? Value { get; set; }
    }

    public enum ConsumerEventType
    {
        AppStart,
        Redemption,
        ProductPurchase,
        RewardActivation,
        PointsSpend
    }
}
