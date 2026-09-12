namespace WINUI.Models;

/// <summary>
/// 下载中心的内容分类。
/// <para>
/// 模组加载器不再单列为一个分类——它随「版本」一起安装，
/// 入口在版本详情页（见 <c>VersionDetailPage</c>）。
/// </para>
/// </summary>
public enum DownloadCategory
{
    /// <summary>游戏版本（可进入详情并安装模组加载器）。</summary>
    GameVersion,

    /// <summary>模组。</summary>
    Mod,

    /// <summary>资源包。</summary>
    ResourcePack,

    /// <summary>光影。</summary>
    Shader,

    /// <summary>世界（地图 / 存档）。</summary>
    World,

    /// <summary>数据包。</summary>
    DataPack,

    /// <summary>整合包。</summary>
    Modpack,
}
