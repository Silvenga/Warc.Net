using System;
using System.Runtime.Serialization;

namespace Warc.Net.Exceptions
{
    [Serializable]
    public class InvalidPayloadLengthDetectedException : Exception
    {
        public InvalidPayloadLengthDetectedException()
        {
        }

        public InvalidPayloadLengthDetectedException(string message) : base(message)
        {
        }

        public InvalidPayloadLengthDetectedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidPayloadLengthDetectedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}