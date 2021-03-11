using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Warc.Net.Parsing;

namespace Warc.Net.Cli.Options.Analyze
{
    public class AnalyzeAction
    {
        private readonly IConsoleWriter _writer;

        public AnalyzeAction(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public async Task Invoke(AnalyzeVerb verb)
        {
            var reader = new WarcReader();

            var bufferingTask = ReadInputs(verb.Inputs, reader);

            var urlsToDomain = new Dictionary<string, HashSet<string>>();
            var totalFiles = 0;

            await foreach (var warcRecord in reader.ReadAllAsync())
            {
                var headers = warcRecord.Header.Fields.ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);
                var type = headers["WARC-Type"];

                if (type != null
                    && type.Equals("response", StringComparison.OrdinalIgnoreCase))
                {
                    var targetUri = headers["WARC-Target-URI"]!;
                    if (targetUri.StartsWith("<") && targetUri.EndsWith(">"))
                    {
                        targetUri = targetUri.Substring(1, targetUri.Length - 2);
                    }

                    var uri = new Uri(targetUri);
                    if (!urlsToDomain.ContainsKey(uri.Host))
                    {
                        urlsToDomain[uri.Host] = new HashSet<string>();
                    }

                    urlsToDomain[uri.Host].Add(uri.PathAndQuery);
                    totalFiles++;
                }
            }

            foreach (var (domain, urls) in urlsToDomain)
            {
                await _writer.Info($"'{domain}': {urls.Count}");
            }

            await _writer.Info($"Total URL's: {totalFiles}");

            await bufferingTask;
        }

        private async Task ReadInputs(IEnumerable<string> inputs, WarcReader reader)
        {
            foreach (var input in inputs)
            {
                await _writer.Info($"Considering input file '{input}'.");
                if (!File.Exists(input))
                {
                    await _writer.Warn($"Input file '{input}' does not exist and will be ignored.");
                }
                else
                {
                    await using var fileStream = File.OpenRead(input);

                    if (input.EndsWith(".gz"))
                    {
                        await using var compressionStream = new GZipStream(fileStream, CompressionMode.Decompress);
                        await reader.WriteAsync(compressionStream);
                    }
                    else
                    {
                        await reader.WriteAsync(fileStream);
                    }

                    await _writer.Info($"Input file '{input}' has been completely buffered.");
                }
            }

            await reader.CompleteWriting();
        }
    }
}