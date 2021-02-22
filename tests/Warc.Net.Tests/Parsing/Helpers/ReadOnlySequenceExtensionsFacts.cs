using System.Buffers;
using System.Text;
using AutoFixture;
using FluentAssertions;
using Warc.Net.Parsing.Helpers;
using Xunit;

namespace Warc.Net.Tests.Parsing.Helpers
{
    public class ReadOnlySequenceExtensionsFacts
    {
        private static readonly Fixture AutoFixture = new();

        [Fact]
        public void When_seq_does_not_contain_double_break_then_PositionOfFirstDoubleBreak_should_return_null()
        {
            var inputStr = AutoFixture.Create<string>();
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.PositionOfFirstDoubleBreak();

            // Assert
            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void When_seq_does_contain_double_break_then_PositionOfFirstDoubleBreak_should_return_position_after_breaks()
        {
            const string inputStr = "foo\r\n\r\nbar";
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.PositionOfFirstDoubleBreak();

            // Assert
            // ReSharper disable once PossibleInvalidOperationException
            inputFake.Slice(result.Value).GetString().Should().Be("bar");
        }

        [Fact]
        public void When_seq_contains_partial_double_break_then_PositionOfFirstDoubleBreak_should_return_null()
        {
            const string inputStr = "foo\r\nbar";
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.PositionOfFirstDoubleBreak();

            // Assert
            result.HasValue.Should().BeFalse();
        }

        [Fact]
        public void When_seq_contains_multiple_breaks_then_PositionOfFirstDoubleBreak_should_return_first_double_break()
        {
            const string inputStr = "foo\r\nbar\r\n\r\ndoo";
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.PositionOfFirstDoubleBreak();

            // Assert
            // ReSharper disable once PossibleInvalidOperationException
            inputFake.Slice(result.Value).GetString().Should().Be("doo");
        }

        [Fact]
        public void When_seq_does_contains_a_value_then_GetString_should_return_string()
        {
            var inputStr = AutoFixture.Create<string>();
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.GetString();

            // Assert
            result.Should().Be(inputStr);
        }

        [Fact]
        public void When_seq_does_contains_a_value_then_GetCharArray_should_return_array()
        {
            var inputStr = AutoFixture.Create<string>();
            var inputFake = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(inputStr));

            // Act
            var result = inputFake.GetCharArray();

            // Assert
            result.Should().BeEquivalentTo(inputStr.ToCharArray());
        }
    }
}