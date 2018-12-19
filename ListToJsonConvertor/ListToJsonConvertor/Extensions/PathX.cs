using System.IO;

namespace ListToJsonConvertor
{
    public static class PathX
    {
        public static string Build(params string[] pathSegments) => Path.GetFullPath(Path.Combine(pathSegments));
    }
}