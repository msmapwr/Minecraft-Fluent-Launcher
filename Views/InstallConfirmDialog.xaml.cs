using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.Services;
using WINUI.ViewModels;

namespace WINUI.Views;

/// <summary>
/// 安装确认对话框：把「点安装」变成「选择 → 确认」，
/// 让用户在开始前看到将安装什么、装到哪里。
/// <para>
/// 按条目类别呈现不同内容（见 XAML 注释）；调用方在 <see cref="ShowAsync"/> 返回
/// <see cref="ContentDialogResult.Primary"/> 后读取 <see cref="Result"/>。
/// </para>
/// </summary>
public sealed partial class InstallConfirmDialog : ContentDialog
{
    /// <summary>确认后的安装选项；仅当结果为 Primary 时有效。</summary>
    public InstallDialogResult? Result { get; private set; }

    /// <summary>条目名称（绑定用）。</summary>
    public string ItemName => _item.Name;

    /// <summary>条目副标题（分类 · 体积 · 版本）。</summary>
    public string ItemSubtitle => $"{_item.CategoryLabel} · {_item.Version} · {_item.SizeLabel}";

    /// <summary>安装位置说明（仅真实安装）。</summary>
    public string LocationText { get; private set; } = string.Empty;

    /// <summary>加载器选项（版本模式）。</summary>
    public IReadOnlyList<LoaderChoiceViewModel> LoaderChoices { get; }

    private readonly DownloadItem _item;
    private readonly InstallDialogMode _mode;
    private readonly string _basePath;

    private InstallConfirmDialog(
        DownloadItem item,
        InstallDialogMode mode,
        IReadOnlyList<LoaderChoiceViewModel> loaderChoices,
        IReadOnlyList<GameVersion> targetVersions,
        IReadOnlyList<LoaderChoiceViewModel> modLoaders,
        string basePath)
    {
        _item = item;
        _mode = mode;
        LoaderChoices = loaderChoices;
        _basePath = basePath;

        InitializeComponent();

        // ---- 按模式装配可见区域与默认值 ----
        var isVersion = mode == InstallDialogMode.GameVersion;
        var isMod = mode == InstallDialogMode.Mod;
        var isModpack = mode == InstallDialogMode.Modpack;

        VersionModePanel.Visibility = isVersion ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        ModModePanel.Visibility = isMod ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        ModpackBar.IsOpen = isModpack;
        TargetInstanceBox.Visibility = isMod ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        LocationPanel.Visibility = isVersion ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        DemoBar.IsOpen = !isVersion;

        Title = $"安装「{item.Name}」";

        if (isVersion)
        {
            LoaderChoiceList.ItemsSource = LoaderChoices;
            LoaderChoiceList.SelectedIndex = 0;
            LocationText = basePath;
        }

        if (isMod)
        {
            ModTargetVersionBox.ItemsSource = targetVersions;
            ModTargetVersionBox.SelectedIndex = 0;
            ModLoaderBox.ItemsSource = modLoaders;
            ModLoaderBox.SelectedIndex = 0;
        }

        InstanceNameBox.Text = DefaultInstanceName(item, mode);

        PrimaryButtonClick += OnPrimaryButtonClicked;

        UpdateLoaderNotice();
        UpdateCompatibility();
        UpdateSummary();
    }

    /// <summary>准备数据并创建对话框（页面 code-behind 调用，await 后 ShowAsync）。</summary>
    public static async Task<InstallConfirmDialog> CreateAsync(DownloadItem item)
    {
        var data = App.GetService<ILauncherDataService>();
        var game = App.GetService<IGameLauncherService>();

        var basePath = game.GamePath.BasePath;

        if (item.IsVersion)
        {
            var loaders = await data.GetLoadersAsync(item.Version);
            var choices = new List<LoaderChoiceViewModel>
            {
                new("Vanilla", "原版（无需加载器）"),
            };

            choices.AddRange(loaders.Select(loader => new LoaderChoiceViewModel(
                loader.Label,
                $"推荐 {loader.RecommendedVersion} · 可在详情页选择具体版本")));

            return new InstallConfirmDialog(item, InstallDialogMode.GameVersion, choices, [], [], basePath);
        }

        if (item.Category == DownloadCategory.Mod)
        {
            var versions = await data.GetVersionsAsync();
            var targetVersions = versions
                .Where(version => version.Channel == VersionChannel.Release)
                .GroupBy(version => version.Id)
                .Select(group => group.First())
                .ToList();

            var modLoaders = new List<LoaderChoiceViewModel>
            {
                new("Fabric", "多数模组的首选加载器"),
                new("Forge", "老牌加载器（≤1.20.x）"),
                new("NeoForge", "Forge 的现代分支（≥1.20.1）"),
            };

            return new InstallConfirmDialog(item, InstallDialogMode.Mod, [], targetVersions, modLoaders, basePath);
        }

        var mode = item.Category == DownloadCategory.Modpack
            ? InstallDialogMode.Modpack
            : InstallDialogMode.Simple;

        return new InstallConfirmDialog(item, mode, [], [], [], basePath);
    }

    private static string DefaultInstanceName(DownloadItem item, InstallDialogMode mode) => mode switch
    {
        InstallDialogMode.GameVersion => $"{item.Version} · 原版",
        InstallDialogMode.Modpack => item.Name,
        InstallDialogMode.Mod => "（安装到所选实例）",
        _ => "（安装到所选实例）",
    };

    /// <summary>当前选中的加载器（版本模式）。</summary>
    private string SelectedLoader => LoaderChoiceList.SelectedItem is LoaderChoiceViewModel choice
        ? choice.DisplayName
        : "Vanilla";

    /// <summary>模组模式下的目标版本号。</summary>
    private string? SelectedModTargetVersion => ModTargetVersionBox.SelectedItem is GameVersion version
        ? version.Id
        : null;

    private void OnLoaderSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateLoaderNotice();
        UpdateSummary();
    }

    private void OnModSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateCompatibility();
        UpdateSummary();
    }

    private void OnInstanceNameChanged(object sender, TextChangedEventArgs e)
    {
        // 实例名不能为空：空时禁用主按钮。
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(InstanceNameBox.Text);
        UpdateSummary();
    }

    /// <summary>非 Vanilla 加载器：说明 v0.3 前按原版安装（方案 2-A）。</summary>
    private void UpdateLoaderNotice()
    {
        if (_mode != InstallDialogMode.GameVersion)
        {
            return;
        }

        LoaderNotice.IsOpen = SelectedLoader != "Vanilla";
    }

    /// <summary>模组兼容性预判：主次版本一致视为兼容，否则黄色警告（不阻断）。</summary>
    private void UpdateCompatibility()
    {
        if (_mode != InstallDialogMode.Mod)
        {
            return;
        }

        var target = SelectedModTargetVersion;
        CompatBar.IsOpen = target is not null
            && !MajorMinor(target!).Equals(MajorMinor(_item.Version), StringComparison.Ordinal);
    }

    /// <summary>提取主次版本（<c>1.21.4</c> → <c>1.21</c>，<c>1.21.x</c> → <c>1.21</c>）。</summary>
    private static string MajorMinor(string version)
    {
        var parts = version.Split('.');
        return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : version;
    }

    private void UpdateSummary()
    {
        if (_mode == InstallDialogMode.GameVersion)
        {
            SummaryTextBlock.Text = $"将安装：Minecraft {_item.Version} · 加载器：{SelectedLoader}"
                + (SelectedLoader != "Vanilla" ? "（本次先装原版）" : string.Empty)
                + $"\n实例名称：{InstanceNameBox.Text}";
        }
        else if (_mode == InstallDialogMode.Mod)
        {
            var loader = ModLoaderBox.SelectedItem is LoaderChoiceViewModel choice ? choice.DisplayName : "—";
            var version = SelectedModTargetVersion ?? "—";
            SummaryTextBlock.Text = $"将下载：{_item.Name}（{_item.SizeLabel}）\n目标版本：{version} · 加载器：{loader}";
        }
        else if (_mode == InstallDialogMode.Modpack)
        {
            SummaryTextBlock.Text = $"将创建实例：{InstanceNameBox.Text}\n依赖（版本 + 加载器）自动安装";
        }
        else
        {
            SummaryTextBlock.Text = $"将下载：{_item.Name}（{_item.SizeLabel}）";
        }
    }

    private void OnPrimaryButtonClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(InstanceNameBox.Text))
        {
            args.Cancel = true;
            return;
        }

        Result = _mode switch
        {
            InstallDialogMode.GameVersion => new InstallDialogResult(SelectedLoader, _item.Version, InstanceNameBox.Text.Trim()),
            InstallDialogMode.Mod => new InstallDialogResult(
                ModLoaderBox.SelectedItem is LoaderChoiceViewModel modLoader ? modLoader.DisplayName : "Fabric",
                SelectedModTargetVersion,
                InstanceNameBox.Text.Trim()),
            _ => new InstallDialogResult("—", null, InstanceNameBox.Text.Trim()),
        };
    }
}
