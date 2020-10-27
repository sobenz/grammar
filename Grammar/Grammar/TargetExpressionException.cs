using System;
using System.Runtime.Serialization;

namespace TargetingTestApp.Grammar
{
    public class TargetExpressionException : Exception
    {
        public string ReferenceCode { get; }

        public TargetExpressionException(string referenceCode, string message) : base(message)
        {
            ReferenceCode = referenceCode;
        }

        public TargetExpressionException(string referenceCode, string message, Exception innerException) : base(message, innerException)
        {
            ReferenceCode = referenceCode;
        }

        public TargetExpressionException(SerializationInfo info, StreamingContext context): base(info, context)
        {
            ReferenceCode = info.GetString(nameof(ReferenceCode));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ReferenceCode), ReferenceCode);
        }
    }
}
