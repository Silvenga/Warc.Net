namespace Warc.Net.Models
{
    public record WarcRecord
    {
        public WarcRecordHeader Header { get; init; }

        public WarcRecordPayload Payload { get; set; }

        public WarcRecord(WarcRecordHeader header, WarcRecordPayload payload)
        {
            Header = header;
            Payload = payload;
        }
    }
}