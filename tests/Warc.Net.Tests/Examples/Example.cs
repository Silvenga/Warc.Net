using System.IO;

namespace Warc.Net.Tests.Examples
{
    public static class Example
    {
        public static string Get(string name)
        {
            var assembly = typeof(Example).Assembly;
            var resourceName = assembly.GetName().Name + ".Examples." + name;

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);
            var result = reader.ReadToEnd();
            return result;
        }
    }
}