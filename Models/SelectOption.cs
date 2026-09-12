namespace WINUI.Models;

/// <summary>下拉框等控件使用的通用选项。</summary>
/// <typeparam name="T">选项值的类型。</typeparam>
/// <param name="Value">选项值。</param>
/// <param name="DisplayName">展示文案。</param>
public sealed record SelectOption<T>(T Value, string DisplayName);
