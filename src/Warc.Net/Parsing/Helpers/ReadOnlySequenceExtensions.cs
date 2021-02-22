﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Warc.Net.Parsing.Helpers
{
    public static class ReadOnlySequenceExtensions
    {
        private static readonly IReadOnlyList<byte> DoubleBreak = new[]
        {
            (byte) '\r',
            (byte) '\n',
            (byte) '\r',
            (byte) '\n'
        };

        public static SequencePosition? PositionOfFirstDoubleBreak(this in ReadOnlySequence<byte> sequence)
        {
            var reader = new SequenceReader<byte>(sequence);
            if (reader.TryAdvanceTo(DoubleBreak[0])
                && reader.Remaining >= DoubleBreak.Count - 1)
            {
                for (var i = 1; i < DoubleBreak.Count; i++)
                {
                    var nextDelimiter = DoubleBreak[i];
                    if (!reader.TryRead(out var next)
                        || nextDelimiter != next)
                    {
                        return null;
                    }
                }

                return reader.Position;
            }

            return null;
        }

        public static string GetString(this in ReadOnlySequence<byte> payload, Encoding? encoding = null)
        {
            // https://stackoverflow.com/a/63824568/2001966

            encoding ??= Encoding.UTF8;
            return payload.IsSingleSegment
                ? encoding.GetString(payload.FirstSpan)
                : GetStringSlow(payload, encoding);

            static string GetStringSlow(in ReadOnlySequence<byte> payload, Encoding encoding)
            {
                var length = checked((int) payload.Length);
                var oversized = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    payload.CopyTo(oversized);
                    return encoding.GetString(oversized, 0, length);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(oversized);
                }
            }
        }
    }
}