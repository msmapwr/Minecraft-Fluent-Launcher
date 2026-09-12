using WINUI.Models;
using WINUI.Services;

namespace WINUI.Tests.Helpers;

/// <summary>
/// 内存版设置服务替身：不落盘，只统计 <see cref="Save"/> 调用次数。
/// </summary>
public sealed class FakeSettingsService : ISettingsService
{
    public FakeSettingsService(AppSettings? settings = null)
    {
        Settings = settings ?? new AppSettings();
    }

    /// <inheritdoc />
    public AppSettings Settings { get; }

    /// <inheritdoc />
    public string DataDirectory => "(fake)";

    /// <summary><see cref="Save"/> 被调用的次数。</summary>
    public int SaveCount { get; private set; }

    /// <inheritdoc />
    public void Save() => SaveCount++;
}
