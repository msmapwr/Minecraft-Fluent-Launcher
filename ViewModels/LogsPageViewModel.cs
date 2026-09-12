using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 日志页的视图模型。
/// <para>
/// 日志来自 <see cref="ILauncherDataService"/>（Mock）。级别筛选按「选定级别及以上」工作，
/// 例如选择「警告及以上」时只显示 WARN 与 ERROR。文本复制使用真实剪贴板。
/// </para>
/// </summary>
public sealed partial class LogsPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;
    private readonly IClipboardService _clipboardService;

    /// <summary>全量日志（筛选的数据源）。</summary>
    private readonly List<LogEntry> _allEntries = [];

    /// <summary>最低显示级别。</summary>
    [ObservableProperty]
    public partial SelectOption<AppLogLevel> SelectedLevel { get; set; }

    /// <summary>搜索关键字。</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; }

    /// <summary>是否自动滚动到最新一条。</summary>
    [ObservableProperty]
    public partial bool AutoScroll { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>筛选后的日志。</summary>
    public ObservableCollection<LogEntry> Entries { get; } = [];

    /// <summary>可选级别门槛。</summary>
    public IReadOnlyList<SelectOption<AppLogLevel>> Levels { get; } =
    [
        new(AppLogLevel.Trace, "全部级别"),
        new(AppLogLevel.Debug, "调试及以上"),
        new(AppLogLevel.Info, "常规及以上"),
        new(AppLogLevel.Warning, "警告及以上"),
        new(AppLogLevel.Error, "仅错误"),
    ];

    /// <summary>列表是否为空。</summary>
    public bool IsEmpty => Entries.Count == 0;

    /// <summary>统计摘要。</summary>
    public string Summary => _allEntries.Count == 0
        ? "暂无日志"
        : $"显示 {Entries.Count} / {_allEntries.Count} 条 · 错误 {_allEntries.Count(entry => entry.IsError)} · 警告 {_allEntries.Count(entry => entry.IsWarning)}";

    public LogsPageViewModel(ILauncherDataService dataService, IClipboardService clipboardService)
    {
        _dataService = dataService;
        _clipboardService = clipboardService;

        SearchText = string.Empty;
        AutoScroll = true;
        StatusMessage = "准备就绪";
        SelectedLevel = Levels[0];

        // Mock 实现返回已完成的 Task，此处会同步跑完。
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _allEntries.AddRange(await _dataService.GetLogEntriesAsync());
        ApplyQuery();
    }

    partial void OnSearchTextChanged(string value) => ApplyQuery();

    partial void OnSelectedLevelChanged(SelectOption<AppLogLevel> value) => ApplyQuery();

    /// <summary>按级别门槛与关键字筛选日志。</summary>
    private void ApplyQuery()
    {
        if (SelectedLevel is null)
        {
            return;
        }

        var query = _allEntries.Where(entry => entry.Level >= SelectedLevel.Value);

        var keyword = SearchText is null ? string.Empty : SearchText.Trim();
        if (keyword.Length > 0)
        {
            query = query.Where(entry =>
                entry.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                entry.Source.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        Entries.Clear();
        foreach (var entry in query)
        {
            Entries.Add(entry);
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(Summary));

        StatusMessage = Entries.Count == 0
            ? "没有符合筛选条件的日志"
            : $"显示 {Entries.Count} 条日志";
    }

    /// <summary>复制当前筛选结果到剪贴板。</summary>
    [RelayCommand]
    private void CopyAll()
    {
        if (Entries.Count == 0)
        {
            StatusMessage = "没有可复制的日志";
            return;
        }

        var builder = new StringBuilder();
        foreach (var entry in Entries)
        {
            builder.Append(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                   .Append(" [").Append(entry.LevelShortLabel).Append(']')
                   .Append(" [").Append(entry.Source).Append("] ")
                   .AppendLine(entry.Message);
        }

        StatusMessage = _clipboardService.TrySetText(builder.ToString())
            ? $"已复制 {Entries.Count} 条日志到剪贴板"
            : "复制失败：剪贴板被其他程序占用，请重试";
    }

    /// <summary>清空当前视图中的日志（不删除磁盘文件）。</summary>
    [RelayCommand]
    private void Clear()
    {
        _allEntries.Clear();
        ApplyQuery();
        StatusMessage = "已清空日志视图（演示，未删除磁盘日志文件）";
    }

    /// <summary>打开日志目录（演示）。</summary>
    [RelayCommand]
    private void OpenLogFolder()
        => StatusMessage = "（演示）将打开日志目录";

    /// <summary>重新载入日志（演示）。</summary>
    [RelayCommand]
    private void Refresh()
    {
        _allEntries.Clear();
        _ = InitializeAsync();
        StatusMessage = "已重新载入日志（演示）";
    }
}
