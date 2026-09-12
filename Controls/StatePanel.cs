using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI.Models;

namespace WINUI.Controls;

/// <summary>
/// 通用状态容器：在正常内容之上叠放「加载中 / 空 / 错误」三层。
/// <para>
/// 用法（样式见 <c>Themes/StatePanel.xaml</c> 的 <c>AppStatePanelStyle</c>）：
/// </para>
/// <code>
/// &lt;controls:StatePanel
///     Style="{StaticResource AppStatePanelStyle}"
///     State="{x:Bind ViewModel.State, Mode=OneWay}"
///     EmptyTitle="{x:Bind ViewModel.EmptyTitle, Mode=OneWay}"
///     EmptyText="{x:Bind ViewModel.EmptyText, Mode=OneWay}"
///     ErrorText="{x:Bind ViewModel.ErrorMessage, Mode=OneWay}"
///     RetryCommand="{x:Bind ViewModel.ReloadCommand}"&gt;
///     &lt;!-- 这里放原本的列表 / 网格 --&gt;
/// &lt;/controls:StatePanel&gt;
/// </code>
/// <para>
/// <see cref="ContentControl.Content"/> 承载正常内容；四态通过
/// <see cref="VisualStateManager"/> 切换可见性，因此切换不产生额外布局开销。
/// </para>
/// </summary>
public sealed class StatePanel : ContentControl
{
    public StatePanel()
    {
        // 让控件默认不参与 Tab 焦点序列，避免 Tab 时停在一个纯容器上。
        IsTabStop = false;
    }

    // ==================== 状态 ====================

    /// <summary>当前状态。</summary>
    public static readonly DependencyProperty StateProperty = Register(
        nameof(State),
        typeof(PageState),
        PageState.Content,
        OnStateChanged);

    /// <summary>当前状态，决定显示内容层还是加载 / 空 / 错误层。</summary>
    public PageState State
    {
        get => (PageState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    // ==================== 文案 ====================

    /// <summary>加载中提示文案。</summary>
    public static readonly DependencyProperty LoadingTextProperty = Register(
        nameof(LoadingText), typeof(string), "正在加载…");

    /// <summary>加载中提示文案。</summary>
    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    /// <summary>空状态图标字形（Segoe Fluent Icons 码点）。</summary>
    public static readonly DependencyProperty EmptyGlyphProperty = Register(
        nameof(EmptyGlyph), typeof(string), "\uE7B8");

    /// <summary>空状态图标字形。</summary>
    public string EmptyGlyph
    {
        get => (string)GetValue(EmptyGlyphProperty);
        set => SetValue(EmptyGlyphProperty, value);
    }

    /// <summary>空状态标题。</summary>
    public static readonly DependencyProperty EmptyTitleProperty = Register(
        nameof(EmptyTitle), typeof(string), "暂无内容");

    /// <summary>空状态标题。</summary>
    public string EmptyTitle
    {
        get => (string)GetValue(EmptyTitleProperty);
        set => SetValue(EmptyTitleProperty, value);
    }

    /// <summary>空状态补充说明。</summary>
    public static readonly DependencyProperty EmptyTextProperty = Register(
        nameof(EmptyText), typeof(string), string.Empty);

    /// <summary>空状态补充说明。</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>错误状态图标字形。</summary>
    public static readonly DependencyProperty ErrorGlyphProperty = Register(
        nameof(ErrorGlyph), typeof(string), "\uEA39");

    /// <summary>错误状态图标字形。</summary>
    public string ErrorGlyph
    {
        get => (string)GetValue(ErrorGlyphProperty);
        set => SetValue(ErrorGlyphProperty, value);
    }

    /// <summary>错误状态标题。</summary>
    public static readonly DependencyProperty ErrorTitleProperty = Register(
        nameof(ErrorTitle), typeof(string), "加载失败");

    /// <summary>错误状态标题。</summary>
    public string ErrorTitle
    {
        get => (string)GetValue(ErrorTitleProperty);
        set => SetValue(ErrorTitleProperty, value);
    }

    /// <summary>错误状态说明。</summary>
    public static readonly DependencyProperty ErrorTextProperty = Register(
        nameof(ErrorText), typeof(string), string.Empty);

    /// <summary>错误状态说明。</summary>
    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    // ==================== 重试 ====================

    /// <summary>重试按钮文案。</summary>
    public static readonly DependencyProperty RetryTextProperty = Register(
        nameof(RetryText), typeof(string), "重试");

    /// <summary>重试按钮文案。</summary>
    public string RetryText
    {
        get => (string)GetValue(RetryTextProperty);
        set => SetValue(RetryTextProperty, value);
    }

    /// <summary>重试命令；为 <c>null</c> 时错误层不显示重试按钮。</summary>
    public static readonly DependencyProperty RetryCommandProperty = Register(
        nameof(RetryCommand), typeof(ICommand), null, OnRetryCommandChanged);

    /// <summary>重试命令。</summary>
    public ICommand? RetryCommand
    {
        get => (ICommand?)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    /// <summary>
    /// 重试按钮的可见性。由 <see cref="RetryCommand"/> 是否为空推导得出，
    /// 这样模板里无需布尔到 Visibility 的转换器。
    /// </summary>
    public static readonly DependencyProperty RetryVisibilityProperty = Register(
        nameof(RetryVisibility), typeof(Visibility), Visibility.Collapsed);

    /// <summary>重试按钮的可见性。</summary>
    public Visibility RetryVisibility
    {
        get => (Visibility)GetValue(RetryVisibilityProperty);
        private set => SetValue(RetryVisibilityProperty, value);
    }

    // ==================== 内部 ====================

    private static DependencyProperty Register(
        string name,
        System.Type propertyType,
        object? defaultValue,
        PropertyChangedCallback? onChanged = null)
        => DependencyProperty.Register(
            name,
            propertyType,
            typeof(StatePanel),
            new PropertyMetadata(defaultValue, onChanged));

    private static void OnRetryCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((StatePanel)d).RetryVisibility = e.NewValue is null ? Visibility.Collapsed : Visibility.Visible;

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((StatePanel)d).ApplyState();

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 模板可能在绑定赋值之前就装载完成，这里补一次状态同步。
        ApplyState();
    }

    /// <summary>把当前状态映射到模板中的 VisualState。</summary>
    private void ApplyState()
        => VisualStateManager.GoToState(this, State switch
        {
            PageState.Loading => "LoadingState",
            PageState.Empty => "EmptyState",
            PageState.Error => "ErrorState",
            _ => "ContentState",
        }, false);
}
