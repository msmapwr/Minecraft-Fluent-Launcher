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
/// </summary>
public sealed partial class ModsPageViewModel : ObservableObject
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

    /// <summary>列表是否为空。</summary>
    public bool IsEmpty => Mods.Count == 0;

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

    public ModsPageViewModel(ILauncherDataService dataService)
    {
        _dataService = dataService;

        SearchText = string.Empty;
        StatusMessage = "准备就绪";
        SelectedFilter = Filters[0];

        // Mock 实现返回已完成的 Task，此处会同步跑完。
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var instances = await _dataService.GetInstancesAsync();
        foreach (var instance in instances)
        {
            Instances.Add(new SelectOption<GameInstance>(
                instance,
                $"{instance.Name}（{instance.GameVersion}）"));
        }

        SelectedInstance = Instances.FirstOrDefault();

        if (SelectedInstance is null)
        {
            ApplyQuery();
        }
    }

    private async Task LoadModsAsync(GameInstance? instance)
    {
        foreach (var mod in _allMods)
        {
            mod.PropertyChanged -= OnModPropertyChanged;
        }

        _allMods.Clear();

        if (instance is not null)
        {
            foreach (var entry in await _dataService.GetModsAsync(instance.Id))
            {
                var item = new ModItemViewModel(entry);
                item.PropertyChanged += OnModPropertyChanged;
                _allMods.Add(item);
            }
        }

        // 集合重建统一走推迟刷新（该调用可能位于下拉框绑定回调的同步续体中）。
        RequestRefresh();
        StatusMessage = instance is null
            ? "请先选择一个实例"
            : $"已加载「{instance.Name}」的模组列表";
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

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EnabledCount));
        OnPropertyChanged(nameof(UpdatableCount));
        OnPropertyChanged(nameof(Summary));
    }

    /// <summary>启用全部模组（演示）。</summary>
    [RelayCommand]
    private void EnableAll()
    {
        foreach (var mod in _allMods)
        {
            mod.IsEnabled = true;
        }

        StatusMessage = "已启用全部模组（演示，未改动文件）";
    }

    /// <summary>禁用全部模组（演示）。</summary>
    [RelayCommand]
    private void DisableAll()
    {
        foreach (var mod in _allMods)
        {
            mod.IsEnabled = false;
        }

        StatusMessage = "已禁用全部模组（演示，未改动文件）";
    }

    /// <summary>检查更新（演示）。</summary>
    [RelayCommand]
    private void CheckUpdates()
    {
        StatusMessage = UpdatableCount == 0
            ? "全部模组均为最新版本（演示）"
            : $"发现 {UpdatableCount} 个可更新模组（演示，未下载）";
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
