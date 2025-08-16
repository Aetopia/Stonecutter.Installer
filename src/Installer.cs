using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using Windows.Win32.UI.Shell;
using static System.Environment;

static class Installer
{
    const string Address = "https://github.com/Aetopia/Stonecutter/releases/download/v{0}/Stonecutter.zip";

    const string Name = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Stonecutter";

    static readonly string _address = string.Format(Address, Metadata.Version);

    static readonly int _count = SystemPageSize;

    static readonly string _temp = Path.GetTempPath();

    static readonly string _shortcut = Path.Combine(GetFolderPath(SpecialFolder.Programs), "Stonecutter.lnk");

    static readonly string _installation = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), @"Programs\Stonecutter");

    internal static void Install()
    {
        Directory.CreateDirectory(_installation);

        var uninstaller = Path.Combine(_installation, $"Stonecutter.Installer.exe");
        try { File.Copy(Metadata.Executable, uninstaller, true); } catch { }

        using var key = Registry.CurrentUser.CreateSubKey(Name);

        key.SetValue("DisplayIcon", Path.Combine(_installation, "Stonecutter.exe"));
        key.SetValue("DisplayName", "Stonecutter");
        key.SetValue("DisplayVersion", Metadata.Version);
        key.SetValue("Publisher", "Aetopia");

        key.SetValue("NoModify", 1);
        key.SetValue("NoRepair", 1);
        key.SetValue("UninstallString", $"\"{uninstaller}\" /Uninstall");

        using var stream = Metadata.GetStream("Stonecutter.zip");
        using ZipArchive archive = new(stream);

        foreach (var entry in archive.Entries)
            entry.ExtractToFile(Path.Combine(_installation, entry.Name), true);

        var shellLink = (IShellLinkW)new ShellLink();

        unsafe
        {
            fixed (char* @string = Path.Combine(_installation, "Stonecutter.exe"))
                shellLink.SetPath(@string);

            fixed (char* @string = "Fixes various bugs related to Minecraft: Bedrock Edition.")
                shellLink.SetDescription(@string);

            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(_shortcut, true);
        }
    }

    internal static void Uninstall()
    {
        if (!_temp.StartsWith(Path.GetDirectoryName(Metadata.Executable), StringComparison.OrdinalIgnoreCase))
        {
            var path = Path.Combine(_temp, $"{Path.GetRandomFileName()}.exe");
            File.Copy(Metadata.Executable, path);

            using (Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = path,
                Arguments = "/Uninstall"
            })) { }
            Exit(0);
        }

        try { Directory.Delete(_installation, true); } catch { }
        try { File.Delete(_shortcut); } catch { }
        Registry.CurrentUser.DeleteSubKeyTree(Name, false);
    }
}