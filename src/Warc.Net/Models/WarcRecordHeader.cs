using System;
using System.Collections.Generic;

namespace Warc.Net.Models
{
    public record WarcRecord
    {
        public string RecordId { get; }

        public IReadOnlyCollection<NamedField> Fields { get; init; } = new List<NamedField>();

        public WarcRecord(string recordId)
        {
            RecordId = recordId;
        }
    }

    public record WarcRecordHeader
    {
        public Version Version { get; init; }

        public IReadOnlyCollection<NamedField> Fields { get; init; }

        public WarcRecordHeader(Version version, IReadOnlyCollection<NamedField> fields)
        {
            Version = version;
            Fields = fields;
        }
    }

    public record NamedField
    {
        public string Name { get; init; }

        public string? Value { get; init; }

        public NamedField(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}