using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>条目安装状态（三态）。</summary>
public enum ItemInstallState
{
    /// <summary>未安装。</summary>
    NotInstalled,

    /// <summary>安装中（队列推进中）。</summary>
    Installing,

    /// <summary>已安装。</summary>
    Installed,
}

/// <summary>
/// 下载中心一个条目的界面投影。
/// <para>
/// 包装不可变的 <see cref="DownloadItem"/>，叠加<b>真实安装状态</b>：
/// 「版本」条目按本地版本目录判定（<c>IGameLauncherService.IsInstalledLocally</c>），
/// 队列推进时进入「安装中」。项目既有模式：需要可变 UI 状态时用 VM 包装模型。
/// </para>
/// </summary>
public sealed partial class DownloadItemViewModel : ObservableObject
{
    /// <summary>来源条目。</summary>
    public DownloadItem Item { get; }

    /// <summary>安装状态。</summary>
    [ObservableProperty]
    public partial ItemInstallState InstallState { get; set; }

    public DownloadItemViewModel(DownloadItem item)
    {
        Item = item;
        InstallState = ItemInstallState.NotInstalled;
    }

    /// <summary>是否已安装（徽章显示）。</summary>
    public bool IsInstalled => InstallState == ItemInstallState.Installed;

    /// <summary>是否安装中（徽章显示）。</summary>
    public bool IsInstalling => InstallState == ItemInstallState.Installing;

    /// <summary>是否未安装（安装按钮显示）。</summary>
    public bool IsNotInstalled => InstallState == ItemInstallState.NotInstalled;

    /// <summary>安装状态标签。</summary>
    public string InstallStateLabel => InstallState switch
    {
        ItemInstallState.Installed => "已安装",
        ItemInstallState.Installing => "安装中…",
        _ => "未安装",
    };

    // ---- 条目展示属性（透传模型） ----

    /// <summary>条目标识。</summary>
    public string Id => Item.Id;

    /// <summary>名称。</summary>
    public string Name => Item.Name;

    /// <summary>简介。</summary>
    public string Description => Item.Description;

    /// <summary>作者 / 来源。</summary>
    public string Author => Item.Author;

    /// <summary>版本 / 适配范围。</summary>
    public string Version => Item.Version;

    /// <summary>分类标签。</summary>
    public string CategoryLabel => Item.CategoryLabel;

    /// <summary>体积标签。</summary>
    public string SizeLabel => Item.SizeLabel;

    /// <summary>下载次数标签。</summary>
    public string DownloadCountLabel => Item.DownloadCountLabel;

    /// <summary>是否为「版本」条目（可进入详情）。</summary>
    public bool IsVersion => Item.IsVersion;

    /// <summary>是否不是「版本」条目（与 <see cref="IsVersion"/> 互斥）。</summary>
    public bool IsNotVersion => !IsVersion;

    /// <summary>同步安装状态（仅变化时触发通知）。</summary>
    public void SetInstallState(ItemInstallState state)
    {
        if (InstallState == state)
        {
            return;
        }

        InstallState = state;
        OnPropertyChanged(nameof(IsInstalled));
        OnPropertyChanged(nameof(IsInstalling));
        OnPropertyChanged(nameof(IsNotInstalled));
        OnPropertyChanged(nameof(InstallStateLabel));
    }

    /// <summary>按「本地是否已装 + 队列是否在装」重算安装状态。</summary>
    public void RefreshInstallState(bool installedLocally, bool installingInQueue)
    {
        var state = installedLocally
            ? ItemInstallState.Installed
            : installingInQueue
                ? ItemInstallState.Installing
                : ItemInstallState.NotInstalled;

        SetInstallState(state);
    }
}
