namespace WINUI.Models;

/// <summary>
/// 模组加载器种类。
/// <para>
/// 只能随「游戏版本」一起安装，入口在版本详情页；
/// 因此不再作为下载中心的独立分类。
/// </para>
/// </summary>
public enum ModLoader
{
    /// <summary>Fabric。</summary>
    Fabric,

    /// <summary>NeoForge（Forge 的社区继任者）。</summary>
    NeoForge,

    /// <summary>Forge。</summary>
    Forge,

    /// <summary>Quilt（Fabric 的衍生分支）。</summary>
    Quilt,
}
