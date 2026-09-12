using System.Text.Json;
using WINUI.Models;
using WINUI.Services;
using Xunit;

namespace WINUI.Tests.Models;

/// <summary>
/// 设置模型：默认值与 JSON 序列化往返（对应 <c>settings.json</c> 的持久化契约）。
/// </summary>
public sealed class AppSettingsTests
{
    [Fact]
    public void Defaults_MatchDocumentedValues()
    {
        var settings = new AppSettings();

        Assert.Equal(1, settings.Version);
        Assert.Equal(AppTheme.System, settings.Theme);
        Assert.Equal("zh-CN", settings.Language);
        Assert.True(settings.EnableAnimations);
        Assert.False(settings.ShowSnapshots);
        Assert.True(settings.AutoCheckUpdates);
        Assert.False(settings.CloseLauncherAfterLaunch);
        Assert.True(settings.AutoDetectJava);
        Assert.Equal(string.Empty, settings.JavaPath);
        Assert.Equal(1024, settings.MinMemoryMb);
        Assert.Equal(4096, settings.MaxMemoryMb);
        Assert.Equal(DownloadSource.Bmclapi, settings.DownloadSource);
        Assert.Equal(8, settings.MaxConcurrentDownloads);
        Assert.Equal(AppLogLevel.Info, settings.LogLevel);
    }

    [Fact]
    public void JsonRoundtrip_PreservesAllValues()
    {
        var original = new AppSettings
        {
            Version = 1,
            Theme = AppTheme.Dark,
            Language = "en-US",
            EnableAnimations = false,
            ShowSnapshots = true,
            AutoCheckUpdates = false,
            CloseLauncherAfterLaunch = true,
            AutoDetectJava = false,
            JavaPath = @"D:\Java\jdk-21\bin\javaw.exe",
            MinMemoryMb = 2048,
            MaxMemoryMb = 8192,
            DownloadSource = DownloadSource.Official,
            MaxConcurrentDownloads = 16,
            LogLevel = AppLogLevel.Debug,
        };

        var json = JsonSerializer.Serialize(original, SettingsJsonContext.Default.AppSettings);
        var restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings);

        Assert.NotNull(restored);
        Assert.Equal(original.Theme, restored.Theme);
        Assert.Equal(original.Language, restored.Language);
        Assert.Equal(original.EnableAnimations, restored.EnableAnimations);
        Assert.Equal(original.ShowSnapshots, restored.ShowSnapshots);
        Assert.Equal(original.AutoCheckUpdates, restored.AutoCheckUpdates);
        Assert.Equal(original.CloseLauncherAfterLaunch, restored.CloseLauncherAfterLaunch);
        Assert.Equal(original.AutoDetectJava, restored.AutoDetectJava);
        Assert.Equal(original.JavaPath, restored.JavaPath);
        Assert.Equal(original.MinMemoryMb, restored.MinMemoryMb);
        Assert.Equal(original.MaxMemoryMb, restored.MaxMemoryMb);
        Assert.Equal(original.DownloadSource, restored.DownloadSource);
        Assert.Equal(original.MaxConcurrentDownloads, restored.MaxConcurrentDownloads);
        Assert.Equal(original.LogLevel, restored.LogLevel);
    }

    [Fact]
    public void JsonRoundtrip_MissingFields_FallBackToDefaults()
    {
        // 旧版本设置文件缺失新增字段时，应回退到默认值而不是抛错。
        // 注意：上下文使用 camelCase 命名策略且默认大小写敏感。
        const string json = "{\"version\":1,\"theme\":1}";

        var restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings);

        Assert.NotNull(restored);
        Assert.Equal(AppTheme.Light, restored.Theme);
        Assert.Equal("zh-CN", restored.Language);
        Assert.Equal(1024, restored.MinMemoryMb);
    }
}
