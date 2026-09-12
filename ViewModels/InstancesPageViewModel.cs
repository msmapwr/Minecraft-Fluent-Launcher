using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 实例列表页的视图模型：搜索 / 筛选 / 排序均在内存中完成（Mock 数据）。
/// <para>
/// 数据来自 <see cref="ILauncherDataService"/>。所有增删改均为 <b>UI 演示</b>，
/// 不会写入磁盘，也不会影响真实实例。
/// </para>
/// <para>
/// 页面状态（加载 / 内容 / 空 / 错误）由 <see cref="PageViewModelBase"/> 提供，
/// 界面统一用 <c>Controls.StatePanel</c> 呈现。
/// </para>
/// </summary>
public sealed partial class InstancesPageViewModel : PageViewModelBase
{
    private readonly ILauncherDataService _dataService;

    /// <summary>全量实例（搜索/筛选的数据源）。</summary>
    private readonly List<GameInstance> _allInstances = [];

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>当前筛选条件。</summary>
    [ObservableProperty]
    public partial SelectOption<InstanceFilter> SelectedFilter { get; set; }

    /// <summary>当前排序方式。</summary>
    [ObservableProperty]
    public partial SelectOption<InstanceSort> SelectedSort { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>筛选后的实例列表（绑定到界面）。</summary>
    public ObservableCollection<GameInstance> Instances { get; } = [];

    /// <summary>可选筛选条件。</summary>
    public IReadOnlyList<SelectOption<InstanceFilter>> Filters { get; } =
    [
        new(InstanceFilter.All, "全部"),
        new(InstanceFilter.Installed, "已安装"),
        new(InstanceFilter.NotInstalled, "未安装"),
    ];

    /// <summary>可选排序方式。</summary>
    public IReadOnlyList<SelectOption<InstanceSort>> Sorts { get; } =
    [
        new(InstanceSort.RecentlyPlayed, "最近游玩"),
        new(InstanceSort.Name, "名称"),
        new(InstanceSort.Size, "占用空间"),
    ];

    /// <summary>总数摘要。</summary>
    public string Summary => _allInstances.Count == 0
        ? "暂无实例"
        : $"共 {_allInstances.Count} 个实例 · 已安装 {_allInstances.Count(instance => instance.IsInstalled)} 个";

    /// <summary>是否一个实例都没有（区别于「筛选后没有匹配」）。</summary>
    private bool HasNoInstances => _allInstances.Count == 0;

    /// <inheritdoc />
    public override string EmptyGlyph => HasNoInstances ? "\uE8F1" : "\uE721";

    /// <inheritdoc />
    public override string EmptyTitle => HasNoInstances ? "还没有实例" : "没有匹配的实例";

    /// <inheritdoc />
    public override string EmptyText => HasNoInstances
        ? "点击右上角「新建实例」创建第一个游戏实例。"
        : "没有符合当前搜索与筛选条件的实例，试试换个关键字。";

    public InstancesPageViewModel(ILauncherDataService dataService)
    {
        _dataService = dataService;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";
        SelectedFilter = Filters[0];
        SelectedSort = Sorts[0];

        _ = ReloadAsync();
    }

    /// <summary>
    /// 从数据源重新载入实例。首次进入、错误状态下的「重试」与手动刷新共用此命令。
    /// </summary>
    [RelayCommand]
    private async Task ReloadAsync()
    {
        var loaded = await RunLoadAsync(async () =>
        {
            var instances = await _dataService.GetInstancesAsync();

            _allInstances.Clear();
            _allInstances.AddRange(instances);
        }, "无法加载实例列表");

        if (!loaded)
        {
            StatusMessage = "实例列表加载失败";
            return;
        }

        ApplyQuery();
        StatusMessage = $"已载入 {_allInstances.Count} 个实例";
    }

    // ComboBox 的 TwoWay 绑定回调可能发生在 XAML 布局过程中，而布局期间不得修改绑定集合
    // （否则抛出 COMException）。故筛选 / 排序 / 搜索变化触发的重建统一推迟到 Dispatcher 回调执行。
    private readonly BoundCollectionUpdater _listUpdater = new();

    /// <summary>请求重新计算筛选结果（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestRefresh() => _listUpdater.Request(ApplyQuery);

    partial void OnSearchTextChanged(string value) => RequestRefresh();

    partial void OnSelectedFilterChanged(SelectOption<InstanceFilter> value) => RequestRefresh();

    partial void OnSelectedSortChanged(SelectOption<InstanceSort> value) => RequestRefresh();

    /// <summary>按当前搜索 / 筛选 / 排序条件刷新列表。</summary>
    private void ApplyQuery()
    {
        // 构造期间 SelectedFilter / SelectedSort 尚未全部赋值，此时先跳过。
        if (SelectedFilter is null || SelectedSort is null)
        {
            return;
        }

        var query = _allInstances.AsEnumerable();

        var keyword = SearchText is null ? string.Empty : SearchText.Trim();
        if (keyword.Length > 0)
        {
            query = query.Where(instance =>
                instance.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                instance.GameVersion.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                instance.Loader.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedFilter.Value switch
        {
            InstanceFilter.Installed => query.Where(instance => instance.IsInstalled),
            InstanceFilter.NotInstalled => query.Where(instance => !instance.IsInstalled),
            _ => query,
        };

        query = SelectedSort.Value switch
        {
            InstanceSort.Name => query.OrderBy(instance => instance.Name, StringComparer.CurrentCulture),
            InstanceSort.Size => query.OrderByDescending(instance => instance.SizeGb),
            _ => query.OrderByDescending(instance => instance.LastPlayedAt ?? DateTimeOffset.MinValue),
        };

        Instances.Clear();
        foreach (var instance in query)
        {
            Instances.Add(instance);
        }

        UpdateContentState(Instances.Count > 0);

        OnPropertyChanged(nameof(EmptyGlyph));
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyText));
        OnPropertyChanged(nameof(Summary));

        StatusMessage = Instances.Count == 0
            ? "没有匹配的实例"
            : $"显示 {Instances.Count} / {_allInstances.Count} 个实例";
    }

    /// <summary>新建实例（演示）。</summary>
    [RelayCommand]
    private void CreateInstance()
    {
        var index = _allInstances.Count + 1;
        var created = new GameInstance
        {
            Id = $"new-instance-{index}",
            Name = $"新建实例 {index}",
            GameVersion = "1.21.4",
            Loader = "Vanilla",
            Channel = VersionChannel.Release,
            PlayTime = TimeSpan.Zero,
            SizeGb = 0,
        };

        _allInstances.Add(created);
        ApplyQuery();
        StatusMessage = $"已创建「{created.Name}」（演示，未写入磁盘）";
    }

    /// <summary>启动指定实例（演示）。</summary>
    public void Launch(GameInstance instance)
    {
        StatusMessage = instance.IsInstalled
            ? $"正在启动「{instance.Name}」…（演示，未拉起进程）"
            : $"「{instance.Name}」尚未安装，请先前往下载中心安装。";
    }

    /// <summary>打开实例目录（演示）。</summary>
    public void OpenFolder(GameInstance instance)
        => StatusMessage = $"（演示）将打开实例目录：{instance.Id}";

    /// <summary>删除指定实例（演示）。</summary>
    public void Delete(GameInstance instance)
    {
        if (_allInstances.Remove(instance))
        {
            ApplyQuery();
            StatusMessage = $"已删除「{instance.Name}」（演示，未删除磁盘文件）";
        }
    }
}
