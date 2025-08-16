using System.IO;
using System.Reflection;

static class Metadata
{
    internal static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

    internal static string Version = Assembly.GetName().Version.ToString(3);

    internal static string Executable = Assembly.ManifestModule.FullyQualifiedName;

    internal static Stream GetStream(string name)
    {
        return Assembly.GetManifestResourceStream(name);
    }
}