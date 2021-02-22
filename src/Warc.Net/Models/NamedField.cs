namespace Warc.Net.Models
{
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