using System.IO;

namespace TableTopSimulatorDeckBuilder.Extensions
{
    public static class PathX
    {
        public static string Build(params string[] pathSegments) => Path.GetFullPath(Path.Combine(pathSegments));
    }
}
