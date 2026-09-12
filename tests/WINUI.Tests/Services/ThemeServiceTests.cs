using System;
using WINUI.Models;
using WINUI.Services;
using WINUI.Tests.Helpers;
using Xunit;

namespace WINUI.Tests.Services;

/// <summary>
/// 主题服务（大更新 ⑤-6 的单一状态源）行为测试。
/// </summary>
public sealed class ThemeServiceTests
{
    [Fact]
    public void Constructor_InitializesCurrentFromSettings()
    {
        var settings = new FakeSettingsService(new AppSettings { Theme = AppTheme.Dark });
        var service = new ThemeService(settings);

        Assert.Equal(AppTheme.Dark, service.Current);
    }

    [Fact]
    public void Options_IsSharedSingleSource_WithThreeEntries()
    {
        var service = new ThemeService(new FakeSettingsService());

        Assert.Equal(3, service.Options.Count);
        Assert.Equal(new[] { AppTheme.System, AppTheme.Light, AppTheme.Dark },
            new[] { service.Options[0].Value, service.Options[1].Value, service.Options[2].Value });
    }

    [Fact]
    public void SetTheme_NewValue_PersistsSavesAndBroadcasts()
    {
        var settings = new FakeSettingsService();
        var service = new ThemeService(settings);

        AppTheme? broadcast = null;
        var broadcastCount = 0;
        service.ThemeChanged += (_, theme) =>
        {
            broadcastCount++;
            broadcast = theme;
        };

        service.SetTheme(AppTheme.Light);

        Assert.Equal(AppTheme.Light, service.Current);
        Assert.Equal(AppTheme.Light, settings.Settings.Theme);
        Assert.Equal(1, settings.SaveCount);
        Assert.Equal(1, broadcastCount);
        Assert.Equal(AppTheme.Light, broadcast);
    }

    [Fact]
    public void SetTheme_SameValue_DoesNotBroadcastOrSave()
    {
        var settings = new FakeSettingsService(new AppSettings { Theme = AppTheme.Light });
        var service = new ThemeService(settings);

        var broadcastCount = 0;
        service.ThemeChanged += (_, _) => broadcastCount++;

        service.SetTheme(AppTheme.Light);

        Assert.Equal(AppTheme.Light, service.Current);
        Assert.Equal(0, broadcastCount);
        Assert.Equal(0, settings.SaveCount);
    }

    [Fact]
    public void SetTheme_RootNotAttached_StillRecordsPreference()
    {
        // 未 Attach 根元素（窗口未创建）时不抛异常，仅记录偏好。
        var settings = new FakeSettingsService();
        var service = new ThemeService(settings);

        service.Attach(null!);

        service.SetTheme(AppTheme.Dark);

        Assert.Equal(AppTheme.Dark, service.Current);
        Assert.Equal(AppTheme.Dark, settings.Settings.Theme);
    }
}
