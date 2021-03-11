using System;
using System.Threading.Tasks;

namespace Warc.Net.Cli
{
    public interface IConsoleWriter
    {
        Task Info(string str);
        Task Warn(string str);
    }

    public class ConsoleWriter : IConsoleWriter
    {
        public async Task Info(string str)
        {
            await Console.Out.WriteLineAsync(str);
        }

        public async Task Warn(string str)
        {
            await Console.Error.WriteLineAsync(str);
        }
    }
}