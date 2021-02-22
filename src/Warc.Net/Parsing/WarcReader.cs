using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Warc.Net.Parsing.Helpers;

namespace Warc.Net.Parsing
{
    public class WarcReader
    {
        private readonly Pipe _pipe = new();

        public async Task Accept(IEnumerable<Stream> streams, CancellationToken cancellationToken = default, bool leaveOpen = false)
        {
            var pipeWriter = _pipe.Writer;
            using var streamGenerator = streams.GetEnumerator();
            do
            {
                var stream = streamGenerator.Current;
                if (stream != null)
                {
                    try
                    {
                        await stream.CopyToAsync(pipeWriter, cancellationToken);
                    }
                    finally
                    {
                        if (!leaveOpen)
                        {
                            await stream.DisposeAsync();
                        }
                    }
                }
            } while (!cancellationToken.IsCancellationRequested
                     && streamGenerator.MoveNext());

            await pipeWriter.CompleteAsync();
        }

        public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pipeReader = _pipe.Reader;

            var result = await pipeReader.ReadAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested
                   && !result.IsCompleted
                   && !result.IsCanceled)
            {
                var buffer = result.Buffer;
                var splitPosition = buffer.PositionOfFirstDoubleBreak();
                if (!splitPosition.HasValue)
                {
                    // Break does not exist in buffer, keep reading into the buffer.
                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                }
                else
                {
                    // Break was found in buffer, split here.
                    var headerBlock = buffer.Slice(buffer.Start, splitPosition.Value);
                    ParseHeader(headerBlock);
                }

                result = await pipeReader.ReadAsync(cancellationToken);
            }

            yield break;
        }

        private static void ParseHeader(in ReadOnlySequence<byte> headerSequence)
        {
            var chars = headerSequence.GetCharArray();

        }
    }
}