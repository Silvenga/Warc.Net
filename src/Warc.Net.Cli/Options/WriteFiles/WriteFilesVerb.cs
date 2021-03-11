using System.Collections.Generic;
using CommandLine;

namespace Warc.Net.Cli.Options.WriteFiles
{
    [Verb("write-files", HelpText = "Write WARC responses to files.")]
    public class WriteFilesVerb
    {
        [Option('i', "input-warc", Required = true, HelpText = "Input WARC file(s).")]
        public IEnumerable<string> Inputs { get; set; } = new List<string>();

        [Option('o', "output-directory", Required = false, HelpText = "Output directory to write files/folder structure to. Defaults to the current working directory.")]
        public string? Output { get; set; }
    }
}