using System.Collections.Generic;

namespace WINUI.Services;

/// <summary>本机检测到的一个 Java 运行时。</summary>
/// <param name="Path">javaw.exe 完整路径。</param>
/// <param name="VersionHint">从目录名推断的版本提示，如 <c>21</c>；未知为 <c>null</c>。</param>
public sealed record JavaInstall(string Path, string? VersionHint);

/// <summary>
/// 本机 Java 运行时检测（大更新 ⑦-3）：JAVA_HOME / 注册表 / 常见安装路径 / PATH。
/// </summary>
public interface IJavaLocatorService
{
    /// <summary>扫描本机全部可用的 javaw.exe。</summary>
    IReadOnlyList<JavaInstall> FindInstalls();

    /// <summary>返回默认可用的 javaw.exe 路径；找不到时为 <c>null</c>。</summary>
    string? FindDefaultJavaPath();
}
