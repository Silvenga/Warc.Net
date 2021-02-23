using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Warc.Net.Parsing;
using Warc.Net.Tests.Examples;
using Xunit;

namespace Warc.Net.Tests.Parsing
{
    public class WarcReaderFacts
    {
        [Fact]
        public async Task Can_read_single_record()
        {
            var inputStr = Example.Get("warcinfo.warc");

            var reader = new WarcReader();
            await reader.WriteAsync(CreateStreamFromString(inputStr));
            await reader.CompleteWriting();

            // Act
            var result = await ToList(reader.ReadAllAsync());

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Can_read_multiple_records()
        {
            var inputStr1 = Example.Get("warcinfo.warc");
            var inputStr2 = Example.Get("warcinfo.warc");

            var reader = new WarcReader();
            await reader.WriteAllAsync(new[]
            {
                CreateStreamFromString(inputStr1),
                CreateStreamFromString(inputStr2),
            });
            await reader.CompleteWriting();

            // Act
            var result = await ToList(reader.ReadAllAsync());

            // Assert
            result.Should().HaveCount(2);
        }

        private static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            await foreach (var e in asyncEnumerable)
            {
                list.Add(e);
            }

            return list;
        }

        private static Stream CreateStreamFromString(string str)
        {
            var memory = new MemoryStream();
            var reader = new StreamWriter(memory);
            reader.Write(str);
            reader.Flush();
            memory.Seek(0, SeekOrigin.Begin);
            return memory;
        }
    }
}