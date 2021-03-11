using System.IO;

namespace Warc.Net.Tests.Examples
{
    public static class Example
    {
        public static Stream Get(string name)
        {
            var assembly = typeof(Example).Assembly;
            var resourceName = assembly.GetName().Name + ".Examples." + name;

            var stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }
    }
}