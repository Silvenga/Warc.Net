﻿using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Pidgin;
using Warc.Net.Exceptions;
using Warc.Net.Models;
using Warc.Net.Parsing.Grammars;
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

            // TODO Empty lists.
            streamGenerator.MoveNext();

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

        public async IAsyncEnumerable<WarcRecord> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pipeReader = _pipe.Reader;

            var result = await pipeReader.ReadAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                var splitPosition = result.Buffer.PositionOfFirstDoubleBreak();
                if (!splitPosition.HasValue)
                {
                    // Break does not exist in buffer, keep reading into the buffer.
                    pipeReader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }
                else
                {
                    // Break was found in buffer, split here.
                    var headerBlock = result.Buffer.Slice(result.Buffer.Start, splitPosition.Value);
                    var header = ParseHeader(headerBlock);

                    // Discard the header and prepare to read the payload.
                    pipeReader.AdvanceTo(splitPosition.Value, result.Buffer.End);

                    while (true)
                    {
                        result = await pipeReader.ReadAsync(cancellationToken);

                        if (result.Buffer.Length >= header.PayloadLength)
                        {
                            break;
                        }

                        if (result.IsCompleted)
                        {
                            throw new InvalidPayloadLengthDetectedException($"Expected a length of {header.PayloadLength} bytes "
                                                                            + $"but buffer contained only {result.Buffer.Length} remaining bytes. "
                                                                            + "This suggests the WARC record is malformed.");
                        }

                        pipeReader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                    }

                    var payloadEnd = result.Buffer.GetPosition(header.PayloadLength);
                    var payloadData = result.Buffer.Slice(result.Buffer.Start, payloadEnd);

                    var payload = new WarcRecordPayload(payloadData.ToArray());

                    yield return new WarcRecord(header, payload);

                    // Seek past the double breaks at the end of the payload.
                    var recordEnd = result.Buffer.GetPosition(4, payloadEnd);
                    pipeReader.AdvanceTo(recordEnd, result.Buffer.End);
                }

                result = await pipeReader.ReadAsync(cancellationToken);

                if (result.IsCompleted && result.Buffer.IsEmpty)
                {
                    yield break;
                }
            }
        }

        private static WarcRecordHeader ParseHeader(in ReadOnlySequence<byte> headerSequence)
        {
            var chars = headerSequence.GetCharArray();
            var header = WarcRecordHeaderGrammar.WarcHeader.ParseOrThrow(chars);
            return header;
        }
    }
}