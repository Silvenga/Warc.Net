using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Warc.Net.Parsing;

namespace Warc.Net.Cli.Options.WriteFiles
{
    public class WriteFilesAction
    {
        private readonly IConsoleWriter _writer;

        public WriteFilesAction(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public async Task Invoke(WriteFilesVerb filesVerb)
        {
            var outputFilePrefix = Path.GetFullPath(filesVerb.Output ?? "./");

            var reader = new WarcReader();

            var bufferingTask = ReadInputs(filesVerb.Inputs, reader);

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

                    var domain = new Uri(targetUri).Host;

                    var targetFilePath = new Uri(targetUri).AbsolutePath;

                    // /post/ -> /post/index.html
                    // / -> /index.html
                    if (targetFilePath.EndsWith("/"))
                    {
                        targetFilePath = targetFilePath + "index.html";
                    }

                    // /index.html -> index.html
                    if (targetFilePath.StartsWith("/"))
                    {
                        targetFilePath = targetFilePath.Substring(1);
                    }

                    // // /index.html -> example.com/index.html
                    targetFilePath = Path.GetFullPath(Path.Join(outputFilePrefix, domain, targetFilePath));

                    if (!targetFilePath.StartsWith(outputFilePrefix))
                    {
                        throw new Exception($"Possible dangerous path transverse detected for '{targetUri}'.");
                    }

                    await _writer.Info($"Writing file '{targetFilePath}' from '{targetUri}'.");

                    if (File.Exists(targetFilePath))
                    {
                        await _writer.Warn($"File '{targetFilePath}' already exists and will be overwritten.");
                    }

                    try
                    {
                        var directory = new FileInfo(targetFilePath).DirectoryName;
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory!);
                        }

                        await using var file = File.OpenWrite(targetFilePath);
                        await file.WriteAsync(warcRecord.Payload.Data);
                    }
                    catch (Exception e)
                    {
                        await _writer.Warn($"An error occurred while writing '{targetFilePath}', moving to next. {e}");
                    }
                }
            }

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