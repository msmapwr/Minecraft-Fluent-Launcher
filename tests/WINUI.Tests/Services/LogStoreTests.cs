using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 进程内日志仓库测试（大更新 ⑦-4）。
/// </summary>
public sealed class LogStoreTests
{
    [Fact]
    public void Log_StoresEntryAndFiresEvent()
    {
        var store = new LogStore();
        LogEntry? received = null;
        store.EntryAdded += entry => received = entry;

        store.Log(AppLogLevel.Info, "Launcher", "hello");

        var entries = store.Entries;
        Assert.Single(entries);
        Assert.Equal("hello", entries[0].Message);
        Assert.Equal("Launcher", entries[0].Source);
        Assert.Same(entries[0], received);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var store = new LogStore();
        store.Log(AppLogLevel.Info, "A", "1");
        store.Log(AppLogLevel.Info, "A", "2");

        store.Clear();

        Assert.Empty(store.Entries);
    }

    [Fact]
    public void Log_RespectsCapacityLimit()
    {
        var store = new LogStore();
        for (var i = 0; i < 5010; i++)
        {
            store.Log(AppLogLevel.Trace, "Test", i.ToString());
        }

        var entries = store.Entries;
        Assert.Equal(5000, entries.Count);
        Assert.Equal("10", entries[0].Message); // 最旧的 10 条被丢弃
        Assert.Equal("5009", entries[^1].Message);
    }
}
