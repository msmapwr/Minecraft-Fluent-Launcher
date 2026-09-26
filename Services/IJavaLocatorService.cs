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

    /// <summary>
    /// 实际执行一次 Java 读取其主版本号（大更新 ⑨-3）。
    /// <para>
    /// 用于启动前校验版本是否满足游戏要求（如 1.20.5+ 需要 Java 21）。
    /// 通过同目录的 <c>java.exe</c> 执行 <c>-version</c> 读取，3 秒超时。
    /// </para>
    /// </summary>
    /// <param name="javawPath">javaw.exe 路径（内部会换成同目录 java.exe）。</param>
    /// <returns>主版本号（如 <c>21</c>；Java 8 返回 <c>8</c>）；无法判定时为 <c>null</c>。</returns>
    int? GetMajorVersion(string javawPath);
}
