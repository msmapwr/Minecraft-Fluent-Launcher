using CommunityToolkit.Mvvm.ComponentModel;

namespace WINUI.ViewModels;

/// <summary>
/// 分页条上的一个数字页码。
/// <para>
/// 同时提供 <see cref="IsCurrent"/> 与 <see cref="IsNotCurrent"/> 两个互斥布尔属性，
/// 便于在 XAML 中直接用内建的 bool → Visibility 转换切换「当前页 / 非当前页」两种按钮外观，
/// 无需引入转换器。
/// </para>
/// </summary>
public sealed partial class PageButtonViewModel : ObservableObject
{
    /// <summary>页码（从 1 开始）。</summary>
    public int Number { get; }

    /// <summary>是否为当前页。</summary>
    [ObservableProperty]
    public partial bool IsCurrent { get; set; }

    /// <summary>是否不是当前页。</summary>
    public bool IsNotCurrent => !IsCurrent;

    /// <summary>按钮文案。</summary>
    public string Label => Number.ToString();

    public PageButtonViewModel(int number, bool isCurrent)
    {
        Number = number;
        IsCurrent = isCurrent;
    }

    partial void OnIsCurrentChanged(bool value) => OnPropertyChanged(nameof(IsNotCurrent));
}
