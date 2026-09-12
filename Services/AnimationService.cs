using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace WINUI.Services;

/// <inheritdoc cref="IAnimationService" />
public sealed class AnimationService : IAnimationService
{
    /// <summary>内容渐入的垂直位移（像素）。</summary>
    private const double EntranceVerticalOffset = 16;

    private readonly ISettingsService _settings;

    public AnimationService(ISettingsService settings)
    {
        _settings = settings;
        IsEnabled = settings.Settings.EnableAnimations;
    }

    /// <inheritdoc />
    public bool IsEnabled { get; private set; }

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public void SetEnabled(bool enabled)
    {
        // 设置实例是全局单例，这里改动会与其他视图模型看到的保持一致。
        if (_settings.Settings.EnableAnimations != enabled)
        {
            _settings.Settings.EnableAnimations = enabled;
            _settings.Save();
        }

        if (IsEnabled == enabled)
        {
            return;
        }

        IsEnabled = enabled;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void ApplyFrameTransition(Frame frame)
    {
        frame.ContentTransitions ??= [];

        frame.ContentTransitions.Clear();

        if (IsEnabled)
        {
            // 页面切换时播放系统自带的「进入 / 退出」过渡。
            frame.ContentTransitions.Add(new NavigationThemeTransition());
        }
    }

    /// <inheritdoc />
    public void ApplyEntrance(UIElement element)
    {
        element.Transitions ??= [];

        element.Transitions.Clear();

        if (!IsEnabled)
        {
            return;
        }

        // 页面内容进入时轻微上移淡入；StaggeringEnabled 让列表项依次出现。
        element.Transitions.Add(new EntranceThemeTransition
        {
            FromVerticalOffset = EntranceVerticalOffset,
            IsStaggeringEnabled = true,
        });
    }
}
