using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace WINUI.Services;

/// <inheritdoc cref="IJavaLocatorService" />
public sealed class JavaLocatorService : IJavaLocatorService
{
    /// <summary>常见安装根目录（含主流发行版：Oracle / Adoptium / Microsoft / Zulu）。</summary>
    private static readonly string[] ScanRoots =
    [
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
    ];

    /// <summary>扫描根目录下的发行版子目录名前缀。</summary>
    private static readonly string[] DistroFolders =
    [
        "Java",
        "Eclipse Adoptium",
        "Eclipse Foundation",
        "Microsoft",
        "Zulu",
        "Amazon Corretto",
        "BellSoft",
    ];

    /// <inheritdoc />
    public IReadOnlyList<JavaInstall> FindInstalls()
    {
        var found = new Dictionary<string, JavaInstall>(StringComparer.OrdinalIgnoreCase);

        // 1) JAVA_HOME
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome))
        {
            TryAdd(found, Path.Combine(javaHome, "bin", "javaw.exe"));
        }

        // 2) 注册表：HKLM\SOFTWARE\JavaSoft\{JDK,Java Runtime Environment}\<version>\RuntimeDir / JavaHome
        foreach (var home in EnumerateRegistryHomes())
        {
            TryAdd(found, Path.Combine(home, "bin", "javaw.exe"));
        }

        // 3) 常见安装路径
        foreach (var root in ScanRoots.Where(root => !string.IsNullOrEmpty(root) && Directory.Exists(root)))
        {
            foreach (var distro in DistroFolders)
            {
                var distroDir = Path.Combine(root, distro);
                if (!Directory.Exists(distroDir))
                {
                    continue;
                }

                foreach (var dir in Directory.EnumerateDirectories(distroDir))
                {
                    TryAdd(found, Path.Combine(dir, "bin", "javaw.exe"));
                }
            }
        }

        // 4) PATH 里的 javaw.exe
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                TryAdd(found, Path.Combine(dir, "javaw.exe"));
            }
            catch (ArgumentException)
            {
                // 非法路径字符：跳过。
            }
        }

        return found.Values.ToList();
    }

    /// <inheritdoc />
    public string? FindDefaultJavaPath() => FindInstalls().FirstOrDefault()?.Path;

    private static void TryAdd(Dictionary<string, JavaInstall> found, string javawPath)
    {
        try
        {
            if (!File.Exists(javawPath) || found.ContainsKey(javawPath))
            {
                return;
            }

            found[javawPath] = new JavaInstall(javawPath, ExtractVersionHint(javawPath));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 无权限 / IO 问题：跳过该候选路径。
        }
    }

    private static IEnumerable<string> EnumerateRegistryHomes()
    {
        foreach (var subKey in new[] { @"SOFTWARE\JavaSoft\JDK", @"SOFTWARE\JavaSoft\Java Runtime Environment" })
        {
            using var key = Registry.LocalMachine.OpenSubKey(subKey);
            if (key is null)
            {
                continue;
            }

            foreach (var version in key.GetSubKeyNames())
            {
                using var versionKey = key.OpenSubKey(version);
                var dir = versionKey?.GetValue("RuntimeDir") as string
                          ?? versionKey?.GetValue("JavaHome") as string;
                if (!string.IsNullOrEmpty(dir))
                {
                    yield return dir;
                }
            }
        }
    }

    /// <summary>从路径里提取版本提示（如 <c>jdk-21.0.3</c> → <c>21.0.3</c>）。</summary>
    private static string? ExtractVersionHint(string javawPath)
    {
        foreach (var segment in javawPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            var name = segment.ToLowerInvariant();
            var index = name.IndexOf("jdk-", StringComparison.Ordinal);
            if (index < 0)
            {
                index = name.IndexOf("jre-", StringComparison.Ordinal);
            }

            if (index >= 0)
            {
                var value = segment.Substring(index + 4);
                return value.Length == 0 ? null : value;
            }
        }

        return null;
    }
}
