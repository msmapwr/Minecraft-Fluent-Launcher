namespace WINUI.Models;

/// <summary>模组运行侧的展示辅助。</summary>
public static class ModSideExtensions
{
    /// <summary>取运行侧的中文标签。</summary>
    public static string ToLabel(this ModSide side) => side switch
    {
        ModSide.Client => "客户端",
        ModSide.Server => "服务端",
        _ => "通用",
    };
}
