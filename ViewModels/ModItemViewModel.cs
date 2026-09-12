using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 模组列表的一项。
/// <para>
/// 模型 <see cref="ModEntry"/> 保持不可变，这里承载界面上可变的「启用 / 禁用」状态。
/// </para>
/// </summary>
public sealed partial class ModItemViewModel : ObservableObject
{
    /// <summary>底层模型。</summary>
    public ModEntry Entry { get; }

    /// <summary>是否启用。</summary>
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public ModItemViewModel(ModEntry entry)
    {
        Entry = entry;
        IsEnabled = entry.IsEnabledByDefault;
    }
}
