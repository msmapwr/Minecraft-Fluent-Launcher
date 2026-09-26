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
/// <param name="Loader">加载器展示名（如 <c>Fabric</c>；只装原版为 <c>Vanilla</c>）。</param>
/// <param name="TargetGameVersion">目标游戏版本（模组等资源使用）。</param>
/// <param name="TargetInstanceName">将创建的实例名。</param>
/// <param name="LoaderKind">加载器种类；<c>null</c> 表示只装原版。</param>
/// <param name="LoaderVersion">将安装的加载器版本。</param>
public sealed record InstallDialogResult(
    string Loader,
    string? TargetGameVersion,
    string TargetInstanceName,
    ModLoader? LoaderKind = null,
    string? LoaderVersion = null);
