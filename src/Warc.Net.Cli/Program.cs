using System;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Warc.Net.Cli.Options.Analyze;
using Warc.Net.Cli.Options.WriteFiles;

namespace Warc.Net.Cli
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var writer = new ConsoleWriter();

            var parser = new Parser(with => with.EnableDashDash = true);
            var result = parser.ParseArguments<WriteFilesVerb, AnalyzeVerb>(args);

            await result.WithParsedAsync<WriteFilesVerb>(verb => WriteFilesVerbAction(verb, writer));
            await result.WithParsedAsync<AnalyzeVerb>(verb => AnalyzeActionAction(verb, writer));
            await result.WithNotParsedAsync(_ => Error(writer));

            Console.WriteLine(HelpText.AutoBuild(result, _ => _, _ => _));
        }

        private static async Task WriteFilesVerbAction(WriteFilesVerb arg, ConsoleWriter writer)
        {
            await new WriteFilesAction(writer).Invoke(arg);
        }

        private static async Task AnalyzeActionAction(AnalyzeVerb arg, ConsoleWriter writer)
        {
            await new AnalyzeAction(writer).Invoke(arg);
        }

        private static async Task Error(ConsoleWriter writer)
        {
            await writer.Warn("Failed to parse command line arguments.");
        }
    }
}