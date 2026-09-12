using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 模组管理页的视图模型。
/// <para>
/// 模组列表来自 <see cref="ILauncherDataService"/>（Mock，当前所有实例返回同一份列表）。
/// 启用 / 禁用与「检查更新」均为 <b>UI 演示</b>，不会改动任何 .jar 文件。
/// </para>
/// <para>
/// 页面状态（加载 / 内容 / 空 / 错误）由 <see cref="PageViewModelBase"/> 提供。
/// </para>
/// </summary>
public sealed partial class ModsPageViewModel : PageViewModelBase
{
    private readonly ILauncherDataService _dataService;

    /// <summary>全量模组（搜索/筛选的数据源）。</summary>
    private readonly List<ModItemViewModel> _allMods = [];

    /// <summary>当前实例。</summary>
    [ObservableProperty]
    public partial SelectOption<GameInstance>? SelectedInstance { get; set; }

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>当前筛选条件。</summary>
    [ObservableProperty]
    public partial SelectOption<ModFilter> SelectedFilter { get; set; }

    /// <summary>当前选中的模组（用于详情面板）。</summary>
    [ObservableProperty]
    public partial ModItemViewModel? SelectedMod { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>可选实例。</summary>
    public ObservableCollection<SelectOption<GameInstance>> Instances { get; } = [];

    /// <summary>当前展示的模组。</summary>
    public ObservableCollection<ModItemViewModel> Mods { get; } = [];

    /// <summary>可选筛选条件。</summary>
    public IReadOnlyList<SelectOption<ModFilter>> Filters { get; } =
    [
        new(ModFilter.All, "全部"),
        new(ModFilter.Enabled, "已启用"),
        new(ModFilter.Disabled, "已禁用"),
        new(ModFilter.Updatable, "可更新"),
    ];

    /// <summary>是否已选中模组。</summary>
    public bool HasSelection => SelectedMod is not null;

    /// <summary>是否未选中任何模组（用于展示提示）。</summary>
    public bool HasNoSelection => SelectedMod is null;

    /// <summary>已启用数量。</summary>
    public int EnabledCount => _allMods.Count(mod => mod.IsEnabled);

    /// <summary>可更新数量。</summary>
    public int UpdatableCount => _allMods.Count(mod => mod.Entry.HasUpdate);

    /// <summary>统计摘要。</summary>
    public string Summary => _allMods.Count == 0
        ? "当前实例没有模组"
        : $"共 {_allMods.Count} 个 · 已启用 {EnabledCount} · 可更新 {UpdatableCount}";

    /// <summary>是否一个实例都没有。</summary>
    private bool HasNoInstances => Instances.Count == 0;

    /// <summary>当前实例是否一个模组都没有（区别于「筛选后没有匹配」）。</summary>
    private bool HasNoMods => _allMods.Count == 0;

    /// <inheritdoc />
    public override string EmptyGlyph => HasNoInstances || HasNoMods ? "\uE7B8" : "\uE721";

    /// <inheritdoc />
    public override string EmptyTitle => HasNoInstances
        ? "还没有可用的实例"
        : HasNoMods ? "当前实例没有模组" : "没有匹配的模组";

    /// <inheritdoc />
    public override string EmptyText => HasNoInstances
        ? "请先在「实例」页创建一个实例，再回来管理模组。"
        : HasNoMods
            ? "把 .jar 文件放进该实例的 mods 目录即可被识别（本页为演示数据）。"
            : "没有符合当前搜索与筛选条件的模组，试试换个关键字。";

    /// <summary>操作结果通知。</summary>
    private readonly IInteractionService _interaction;

    public ModsPageViewModel(ILauncherDataService dataService, IInteractionService interaction)
    {
        _dataService = dataService;
        _interaction = interaction;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";
        SelectedFilter = Filters[0];

        _ = LoadInstancesAsync();
    }

    /// <summary>
    /// 重新载入当前内容。错误状态下的「重试」与页头刷新共用此命令：
    /// 还没有实例列表时先补实例，否则重载当前实例的模组。
    /// </summary>
    [RelayCommand]
    private async Task ReloadAsync()
    {
        if (Instances.Count == 0)
        {
            await LoadInstancesAsync();
            return;
        }

        await LoadModsAsync(SelectedInstance?.Value);
    }

    private async Task LoadInstancesAsync()
    {
        var loaded = await RunLoadAsync(async () =>
        {
            var instances = await _dataService.GetInstancesAsync();

            Instances.Clear();
            foreach (var instance in instances)
            {
                Instances.Add(new SelectOption<GameInstance>(
                    instance,
                    $"{instance.Name}（{instance.GameVersion}）"));
            }
        }, "无法加载实例列表");

        if (!loaded)
        {
            StatusMessage = "实例列表加载失败";
            return;
        }

        SelectedInstance = Instances.FirstOrDefault();

        if (SelectedInstance is null)
        {
            // 没有实例可选：直接进入空状态，等待用户在「实例」页创建。
            State = PageState.Empty;
            RequestRefresh();
            StatusMessage = "还没有可用的实例，请先在「实例」页创建。";
        }
    }

    private async Task LoadModsAsync(GameInstance? instance)
    {
        foreach (var mod in _allMods)
        {
            mod.PropertyChanged -= OnModPropertyChanged;
        }

        _allMods.Clear();

        if (instance is null)
        {
            State = PageState.Empty;
            RequestRefresh();
            StatusMessage = "请先选择一个实例";
            return;
        }

        var loaded = await RunLoadAsync(async () =>
        {
            foreach (var entry in await _dataService.GetModsAsync(instance.Id))
            {
                var item = new ModItemViewModel(entry);
                item.PropertyChanged += OnModPropertyChanged;
                _allMods.Add(item);
            }
        }, $"无法加载「{instance.Name}」的模组列表");

        // 集合重建统一走推迟刷新（该调用可能位于下拉框绑定回调的同步续体中）。
        RequestRefresh();

        StatusMessage = loaded
            ? $"已加载「{instance.Name}」的模组列表（{_allMods.Count} 个）"
            : "模组列表加载失败";
    }

    private void OnModPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ModItemViewModel.IsEnabled))
        {
            return;
        }

        OnPropertyChanged(nameof(EnabledCount));
        OnPropertyChanged(nameof(Summary));

        // 处于「已启用 / 已禁用」筛选时，切换开关需要同步刷新列表。
        // 开关是 Mode=TwoWay 绑定，其回调可能落在布局过程中，故走推迟刷新。
        if (SelectedFilter.Value is ModFilter.Enabled or ModFilter.Disabled)
        {
            RequestRefresh();
        }
    }

    // 绑定回调（实例 / 筛选下拉、搜索框、模组启用开关）可能发生在 XAML 布局过程中，
    // 而布局期间不得修改绑定集合（否则抛出 COMException），故列表重建统一推迟到 Dispatcher 回调。
    private readonly BoundCollectionUpdater _listUpdater = new();

    /// <summary>请求重建模组列表（推迟到 Dispatcher 回调执行）。</summary>
    private void RequestRefresh() => _listUpdater.Request(RefreshMods);

    /// <summary>重建列表，并保证详情面板的选中项落在当前可见的模组上。</summary>
    private void RefreshMods()
    {
        var previous = SelectedMod;

        ApplyQuery();

        if (previous is null || Mods.Count == 0 || !Mods.Contains(previous))
        {
            SelectedMod = Mods.FirstOrDefault();
        }
    }

    partial void OnSearchTextChanged(string value) => RequestRefresh();

    partial void OnSelectedFilterChanged(SelectOption<ModFilter> value) => RequestRefresh();

    partial void OnSelectedInstanceChanged(SelectOption<GameInstance>? value) => _ = LoadModsAsync(value?.Value);

    partial void OnSelectedModChanged(ModItemViewModel? value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(HasNoSelection));
    }

    /// <summary>按当前搜索 / 筛选条件刷新列表。</summary>
    private void ApplyQuery()
    {
        if (SelectedFilter is null)
        {
            return;
        }

        var query = _allMods.AsEnumerable();

        var keyword = SearchText is null ? string.Empty : SearchText.Trim();
        if (keyword.Length > 0)
        {
            query = query.Where(mod =>
                mod.Entry.Name.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                mod.Entry.Author.Contains(keyword, System.StringComparison.OrdinalIgnoreCase) ||
                mod.Entry.Description.Contains(keyword, System.StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedFilter.Value switch
        {
            ModFilter.Enabled => query.Where(mod => mod.IsEnabled),
            ModFilter.Disabled => query.Where(mod => !mod.IsEnabled),
            ModFilter.Updatable => query.Where(mod => mod.Entry.HasUpdate),
            _ => query,
        };

        Mods.Clear();
        foreach (var mod in query)
        {
            Mods.Add(mod);
        }

        UpdateContentState(Mods.Count > 0);

        OnPropertyChanged(nameof(EmptyGlyph));
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyText));
        OnPropertyChanged(nameof(EnabledCount));
        OnPropertyChanged(nameof(UpdatableCount));
        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>启用全部模组（演示）。批量操作会影响全部模组，因此给出明确反馈。</summary>
    [RelayCommand]
    private void EnableAll()
    {
        if (_allMods.Count == 0)
        {
            StatusMessage = "当前没有可操作的模组";
            return;
        }

        foreach (var mod in _allMods)
        {
            mod.IsEnabled = true;
        }

        StatusMessage = "已启用全部模组（演示，未改动文件）";

        _interaction.Notify(
            $"已启用 {_allMods.Count} 个模组（演示，未改动任何文件）",
            NotificationSeverity.Success,
            "已全部启用");
    }

    /// <summary>禁用全部模组（演示）。</summary>
    [RelayCommand]
    private void DisableAll()
    {
        if (_allMods.Count == 0)
        {
            StatusMessage = "当前没有可操作的模组";
            return;
        }

        foreach (var mod in _allMods)
        {
            mod.IsEnabled = false;
        }

        StatusMessage = "已禁用全部模组（演示，未改动文件）";

        _interaction.Notify(
            $"已禁用 {_allMods.Count} 个模组（演示，未改动任何文件）",
            NotificationSeverity.Success,
            "已全部禁用");
    }

    /// <summary>检查更新（演示）。</summary>
    [RelayCommand]
    private void CheckUpdates()
    {
        if (UpdatableCount == 0)
        {
            StatusMessage = "全部模组均为最新版本（演示）";
            _interaction.Notify("全部模组均为最新版本（演示）", NotificationSeverity.Informational, "没有可用更新");
            return;
        }

        StatusMessage = $"发现 {UpdatableCount} 个可更新模组（演示，未下载）";

        _interaction.Notify(
            $"发现 {UpdatableCount} 个可更新的模组（演示，未下载）",
            NotificationSeverity.Informational,
            "检查完成");
    }

    /// <summary>打开模组目录（演示）。</summary>
    [RelayCommand]
    private void OpenModsFolder()
    {
        StatusMessage = SelectedInstance is null
            ? "请先选择一个实例"
            : $"（演示）将打开「{SelectedInstance.Value.Name}」的 mods 目录";
    }
}
