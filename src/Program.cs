using System;
using System.Linq;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Environment.Exit(1);
        if (args.Any(_ => _.Equals("/uninstall", StringComparison.OrdinalIgnoreCase))) Installer.Uninstall();
        else Installer.Install();
    }
}