using System;
using System.IO;
using FluentAssertions;
using Pidgin;
using Warc.Net.Models;
using Warc.Net.Parsing.Grammars;
using Warc.Net.Tests.Examples;
using Xunit;

namespace Warc.Net.Tests.Parsing.Grammars
{
    public class WarcRecordHeaderGrammarFacts
    {
        [Fact]
        public void Can_parse_example_header()
        {
            var input = new StreamReader(Example.Get("warcinfo.warc")).ReadToEnd();

            // Act
            var result = WarcRecordHeaderGrammar.WarcHeader.Parse(input);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        public void Can_parse_version()
        {
            const string input = "WARC/1.1\r\n";

            // Act
            var result = WarcRecordHeaderGrammar.WarcVersion.ParseOrThrow(input);

            // Assert
            result.Should().Be("1.1");
        }

        [Fact]
        public void Can_parse_header()
        {
            const string input = "WARC-Type: warcinfo\r\n";

            // Act
            var result = WarcRecordHeaderGrammar.NamedField.ParseOrThrow(input);

            // Assert
            result.Should().Be(new NamedField("WARC-Type", "warcinfo"));
        }

        [Fact]
        public void Can_parse_multi_lined_header()
        {
            const string input = "name: foo\r\n bar\r\n";

            // Act
            var result = WarcRecordHeaderGrammar.NamedField.ParseOrThrow(input);

            // Assert
            result.Should().Be(new NamedField("name", "foo bar"));
        }

        [Fact]
        public void Can_parse_multiple_headers()
        {
            const string input = "name1: foo\r\nname2: bar\r\n";

            // Act
            var result = WarcRecordHeaderGrammar.NamedFields.ParseOrThrow(input);

            // Assert
            result.Should().HaveCount(2);
        }
    }
}