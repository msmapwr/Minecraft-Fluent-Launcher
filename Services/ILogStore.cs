using System;
using System.Collections.Generic;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 进程内日志仓库（单例）：启动器自身与游戏进程的输出都写到这里，
/// 日志页订阅展示（大更新 ⑦-4）。
/// </summary>
public interface ILogStore
{
    /// <summary>当前日志快照（按写入顺序）。</summary>
    IReadOnlyList<LogEntry> Entries { get; }

    /// <summary>有新日志时触发（UI 线程外触发，订阅方自行调度）。</summary>
    event Action<LogEntry>? EntryAdded;

    /// <summary>写入一条日志。</summary>
    void Log(AppLogLevel level, string source, string message);

    /// <summary>清空全部日志。</summary>
    void Clear();
}
