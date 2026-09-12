namespace WINUI.Models;

/// <summary>模组列表的筛选条件。</summary>
public enum ModFilter
{
    /// <summary>全部模组。</summary>
    All,

    /// <summary>仅已启用。</summary>
    Enabled,

    /// <summary>仅已禁用。</summary>
    Disabled,

    /// <summary>仅可更新。</summary>
    Updatable,
}
