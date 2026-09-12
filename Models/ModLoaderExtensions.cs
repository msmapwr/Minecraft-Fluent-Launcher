namespace WINUI.Models;

/// <summary>模组加载器的展示辅助。</summary>
public static class ModLoaderExtensions
{
    /// <summary>取加载器的展示名。</summary>
    public static string ToLabel(this ModLoader loader) => loader switch
    {
        ModLoader.Fabric => "Fabric",
        ModLoader.NeoForge => "NeoForge",
        ModLoader.Forge => "Forge",
        _ => "Quilt",
    };

    /// <summary>取加载器的一句话说明。</summary>
    public static string ToDescription(this ModLoader loader) => loader switch
    {
        ModLoader.Fabric => "轻量、启动快、更新及时，适合性能向与小型模组。",
        ModLoader.NeoForge => "面向 1.20.1 及以上版本的现代模组加载器，社区活跃。",
        ModLoader.Forge => "历史最悠久的加载器，1.20.1 及以下版本模组生态庞大。",
        _ => "Fabric 的衍生分支，兼容大部分 Fabric 模组并增强依赖管理。",
    };
}
