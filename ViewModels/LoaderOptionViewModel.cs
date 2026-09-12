using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 版本详情页中「一个可安装的模组加载器」的视图模型。
/// <para>包装不可变的 <see cref="LoaderEntry"/>，为界面提供可变的勾选状态与所选版本。</para>
/// </summary>
public sealed partial class LoaderOptionViewModel : ObservableObject
{
    private readonly IReadOnlyList<SelectOption<string>> _versions;

    /// <summary>对应的加载器数据。</summary>
    public LoaderEntry Entry { get; }

    /// <summary>是否勾选该加载器。</summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>所选加载器版本。</summary>
    [ObservableProperty]
    public partial SelectOption<string>? SelectedVersion { get; set; }

    /// <summary>可选的加载器版本。</summary>
    public IReadOnlyList<SelectOption<string>> Versions => _versions;

    /// <summary>加载器名称。</summary>
    public string Label => Entry.Label;

    /// <summary>加载器说明。</summary>
    public string Description => Entry.Description;

    /// <summary>推荐版本标签。</summary>
    public string RecommendedLabel => Entry.RecommendedLabel;

    public LoaderOptionViewModel(LoaderEntry entry)
    {
        Entry = entry;
        _versions = entry.Versions.Select(version => new SelectOption<string>(version, version)).ToList();
        SelectedVersion = _versions.FirstOrDefault(option => option.Value == entry.RecommendedVersion)
            ?? _versions.FirstOrDefault();
    }
}
