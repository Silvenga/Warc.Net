namespace Warc.Net.Models
{
    public record WarcRecordPayload
    {
        public byte[] Data { get; init; }

        public WarcRecordPayload(byte[] data)
        {
            Data = data;
        }
    }
}