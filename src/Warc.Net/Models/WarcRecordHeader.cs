using System;
using System.Collections.Generic;
using System.Linq;

namespace Warc.Net.Models
{
    public record WarcRecordHeader
    {
        public Version Version { get; init; }

        public IReadOnlyCollection<NamedField> Fields { get; init; }

        public int PayloadLength { get; init; }

        public WarcRecordHeader(Version version, IReadOnlyCollection<NamedField> fields)
        {
            Version = version;
            Fields = fields;

            var contentLengthHeader = Fields.Single(x => x.Name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase));
            PayloadLength = int.Parse(contentLengthHeader.Value!);
        }
    }
}