using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 下载中心的资源类型卡片（一级入口）。
/// <para>进入页面默认选中 Minecraft；「当前项 / 非当前项」用互斥布尔切换外观（项目既有模式）。</para>
/// </summary>
public sealed class CategoryCardViewModel
{
    /// <summary>对应分类。</summary>
    public DownloadCategory Category { get; }

    /// <summary>类型名。</summary>
    public string DisplayName { get; }

    /// <summary>图标字形（Icons.xaml）。</summary>
    public string Glyph { get; }

    /// <summary>说明。</summary>
    public string Description { get; }

    /// <summary>是否当前选中的类型。</summary>
    public bool IsCurrent { get; private set; }

    /// <summary>是否不是当前选中的类型（与 <see cref="IsCurrent"/> 互斥）。</summary>
    public bool IsNotCurrent => !IsCurrent;

    public CategoryCardViewModel(DownloadCategory category, string displayName, string glyph, string description)
    {
        Category = category;
        DisplayName = displayName;
        Glyph = glyph;
        Description = description;
        IsCurrent = false;
    }

    /// <summary>更新选中态（供 VM 在选择变化时统一刷新）。</summary>
    public void SetSelected(bool selected)
    {
        IsCurrent = selected;
    }
}
