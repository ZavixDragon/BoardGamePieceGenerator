using System.IO;

namespace Generator
{
    public static class PathX
    {
        public static string Build(params string[] pathSegments) => Path.GetFullPath(Path.Combine(pathSegments));
    }
}
