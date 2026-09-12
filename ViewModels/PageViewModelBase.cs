using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 带「加载 / 内容 / 空 / 错误」四态的页面视图模型基类。
/// <para>
/// 列表页的状态逻辑集中在这里，页面只需要把 <see cref="State"/> 交给
/// <c>Controls.StatePanel</c>，并覆写三个空状态文案属性。
/// </para>
/// <para>
/// 约定：页面一旦进入 <see cref="PageState.Error"/>，会保持错误态直到用户主动重试
/// （<c>RunLoadAsync</c> 再次执行），避免筛选操作把错误提示「顶掉」。
/// </para>
/// </summary>
public abstract partial class PageViewModelBase : ObservableObject
{
    protected PageViewModelBase()
    {
        State = PageState.Loading;
        ErrorMessage = string.Empty;
    }

    /// <summary>当前页面状态。</summary>
    [ObservableProperty]
    public partial PageState State { get; set; }

    /// <summary>错误状态下的说明文案（含失败原因）。</summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; }

    /// <summary>是否正在加载。</summary>
    public bool IsLoading => State == PageState.Loading;

    /// <summary>是否正常展示内容。</summary>
    public bool IsContent => State == PageState.Content;

    /// <summary>是否处于错误状态。</summary>
    public bool HasError => State == PageState.Error;

    /// <summary>空状态标题。</summary>
    public virtual string EmptyTitle => "暂无内容";

    /// <summary>空状态说明，用于区分「尚无数据」与「没有匹配结果」。</summary>
    public virtual string EmptyText => string.Empty;

    /// <summary>空状态图标字形。</summary>
    public virtual string EmptyGlyph => "\uE7B8";

    partial void OnStateChanged(PageState value)
    {
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(IsContent));
        OnPropertyChanged(nameof(HasError));

        OnStateChangedCore();
    }

    /// <summary>状态变化时的额外通知钩子，供子类刷新派生属性。</summary>
    protected virtual void OnStateChangedCore()
    {
    }

    /// <summary>
    /// 执行一次加载：期间进入 <see cref="PageState.Loading"/>；
    /// 发生异常时进入 <see cref="PageState.Error"/> 并记录原因，<b>不向外抛出</b>。
    /// </summary>
    /// <param name="load">实际的数据加载逻辑。</param>
    /// <param name="failurePrefix">错误提示前缀，例如「无法加载下载条目」。</param>
    /// <returns>加载成功返回 <c>true</c>；失败返回 <c>false</c>，调用方应跳过后续的状态刷新。</returns>
    protected async Task<bool> RunLoadAsync(Func<Task> load, string failurePrefix)
    {
        State = PageState.Loading;
        ErrorMessage = string.Empty;

        try
        {
            await load();

            // 先离开 Loading（此时还不知道有没有数据，用中性的 Content 兜底）；
            // 调用方随后应通过 UpdateContentState 收敛为 Content 或 Empty。
            State = PageState.Content;
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{failurePrefix}。{ex.Message}";
            State = PageState.Error;
            return false;
        }
    }

    /// <summary>
    /// 根据「是否有可见项」切换到内容态或空态。
    /// <para>
    /// 加载中不覆盖：避免数据未到位时先闪一下空状态。
    /// 错误态不覆盖：保证错误提示与「重试」不会被筛选操作顶掉。
    /// </para>
    /// </summary>
    /// <param name="hasVisibleItems">当前筛选 / 搜索后是否有可见项。</param>
    protected void UpdateContentState(bool hasVisibleItems)
    {
        if (State is PageState.Loading or PageState.Error)
        {
            return;
        }

        State = hasVisibleItems ? PageState.Content : PageState.Empty;
    }
}
