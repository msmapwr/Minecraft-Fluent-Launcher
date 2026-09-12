using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;
using WINUI.Services;
using WINUI.ViewModels;

namespace WINUI.Controls;

/// <summary>
/// 通知宿主：把 <see cref="IInteractionService"/> 发出的通知渲染为一叠 InfoBar。
/// <para>
/// 放在窗口内容的最上层（右下角），并以 <c>Attach</c> 注入服务——
/// 控件由 XAML 创建、无法构造函数注入，因此不在内部使用服务定位器。
/// </para>
/// </summary>
public sealed partial class NotificationHost : UserControl
{
    /// <summary>等待入列的项。集合写入统一推迟到 Dispatcher 回调，避免布局期间改集合。</summary>
    private readonly Queue<NotificationItemViewModel> _pending = [];

    /// <summary>自动关闭计时器（仅在需要停止时使用）。</summary>
    private readonly Dictionary<NotificationItemViewModel, DispatcherQueueTimer> _closeTimers = [];

    /// <summary>关闭动画结束后再移除该项的宽限时间。</summary>
    private static readonly TimeSpan RemovalGrace = TimeSpan.FromMilliseconds(200);

    private IInteractionService? _interaction;
    private bool _flushQueued;

    /// <summary>当前显示的通知，最新的在最前。</summary>
    public ObservableCollection<NotificationItemViewModel> Items { get; } = [];

    public NotificationHost()
    {
        InitializeComponent();
        Unloaded += OnUnloaded;
    }

    /// <summary>绑定交互服务。重复调用同一实例时不做任何事。</summary>
    /// <param name="interaction">交互服务。</param>
    public void Attach(IInteractionService interaction)
    {
        if (ReferenceEquals(_interaction, interaction))
        {
            return;
        }

        if (_interaction is not null)
        {
            _interaction.NotificationRequested -= OnNotificationRequested;
        }

        _interaction = interaction;
        interaction.NotificationRequested += OnNotificationRequested;
    }

    private void OnNotificationRequested(object? sender, AppNotification notification)
    {
        var item = new NotificationItemViewModel(notification);
        item.PropertyChanged += OnItemPropertyChanged;

        // 通知可能由布局期间触发的绑定回调发出，而 WinUI 禁止在 Measure/Arrange 中
        // 修改绑定集合（否则抛 COMException）。因此统一推迟到下一帧再插入。
        Enqueue(item);

        StartAutoClose(item);
    }

    /// <summary>把一项排入待插入队列，并在下一帧一次性清空队列。</summary>
    private void Enqueue(NotificationItemViewModel item)
    {
        _pending.Enqueue(item);

        if (_flushQueued)
        {
            return;
        }

        _flushQueued = true;

        if (!DispatcherQueue.TryEnqueue(FlushPending))
        {
            // 入队失败（例如队列已关闭）时同步处理，保证通知不丢失。
            _flushQueued = false;
            FlushPending();
        }
    }

    private void FlushPending()
    {
        _flushQueued = false;

        while (_pending.Count > 0)
        {
            // 逐项插入到队首：后插入的更新，因此最新的通知显示在最上方。
            Items.Insert(0, _pending.Dequeue());
        }
    }

    private void StartAutoClose(NotificationItemViewModel item)
    {
        var timer = DispatcherQueue.CreateTimer();
        timer.Interval = item.AutoCloseDelay;
        timer.IsRepeating = false;
        timer.Tick += (_, _) => item.IsOpen = false;

        _closeTimers[item] = timer;
        timer.Start();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NotificationItemViewModel item ||
            e.PropertyName != nameof(NotificationItemViewModel.IsOpen) ||
            item.IsOpen)
        {
            return;
        }

        item.PropertyChanged -= OnItemPropertyChanged;

        if (_closeTimers.Remove(item, out var timer))
        {
            timer.Stop();
        }

        // 留一点时间让 InfoBar 播放收起动画，随后再移除，避免「啪」地消失。
        var removal = DispatcherQueue.CreateTimer();
        removal.Interval = RemovalGrace;
        removal.IsRepeating = false;
        removal.Tick += (_, _) => Items.Remove(item);
        removal.Start();
    }

    private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_interaction is not null)
        {
            _interaction.NotificationRequested -= OnNotificationRequested;
        }

        foreach (var item in Items)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }

        foreach (var timer in _closeTimers.Values)
        {
            timer.Stop();
        }

        _closeTimers.Clear();
        _pending.Clear();
    }
}
