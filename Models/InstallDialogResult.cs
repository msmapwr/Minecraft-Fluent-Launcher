namespace WINUI.Models;

/// <summary>安装确认对话框按条目类别呈现的内容模式。</summary>
public enum InstallDialogMode
{
    /// <summary>游戏版本：加载器单选 + 实例名 + 位置。</summary>
    GameVersion,

    /// <summary>模组：目标版本 + 加载器 + 兼容性。</summary>
    Mod,

    /// <summary>整合包：依赖摘要 + 新实例名。</summary>
    Modpack,

    /// <summary>其它类型：简单确认。</summary>
    Simple,
}

/// <summary>安装确认的结果（调用方读取）。</summary>
public sealed record InstallDialogResult(string Loader, string? TargetGameVersion, string TargetInstanceName);
