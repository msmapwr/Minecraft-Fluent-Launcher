namespace WINUI.Models;

/// <summary>
/// 列表 / 页面的四态，供 <c>Controls.StatePanel</c> 决定展示哪一层。
/// <para>
/// 用「单一枚举 + 一个状态容器」取代过去每个页面各自拼一套空状态 XAML 的做法。
/// </para>
/// </summary>
public enum PageState
{
    /// <summary>正常展示内容。</summary>
    Content,

    /// <summary>正在加载数据。</summary>
    Loading,

    /// <summary>加载完成但没有数据，或没有匹配结果。</summary>
    Empty,

    /// <summary>加载失败。</summary>
    Error,
}
