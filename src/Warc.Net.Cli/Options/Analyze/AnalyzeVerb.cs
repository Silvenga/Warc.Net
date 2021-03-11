using System.Collections.Generic;
using CommandLine;

namespace Warc.Net.Cli.Options.Analyze
{
    [Verb("analyze", HelpText = "Analyze WARC responses.")]
    public class AnalyzeVerb
    {
        [Option('i', "input-warc", Required = true, HelpText = "Input WARC file(s).")]
        public IEnumerable<string> Inputs { get; set; } = new List<string>();
    }
}