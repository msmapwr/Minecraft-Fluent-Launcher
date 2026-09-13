using System;
using System.Collections.Generic;
using WINUI.Models;

namespace WINUI.Services;

/// <inheritdoc cref="ILogStore" />
public sealed class LogStore : ILogStore
{
    /// <summary>内存上限，超过后丢弃最旧的日志。</summary>
    private const int MaxEntries = 5000;

    private readonly object _gate = new();
    private readonly List<LogEntry> _entries = [];

    /// <inheritdoc />
    public event Action<LogEntry>? EntryAdded;

    /// <inheritdoc />
    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public void Log(AppLogLevel level, string source, string message)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Level = level,
            Source = source,
            Message = message,
        };

        lock (_gate)
        {
            _entries.Add(entry);
            if (_entries.Count > MaxEntries)
            {
                _entries.RemoveAt(0);
            }
        }

        EntryAdded?.Invoke(entry);
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
        }
    }
}
