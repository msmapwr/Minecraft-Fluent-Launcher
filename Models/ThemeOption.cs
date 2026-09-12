namespace WINUI.Models;

/// <summary>主题选项：供界面绑定的「主题值 + 显示名」组合。</summary>
/// <param name="Value">主题值。</param>
/// <param name="DisplayName">界面上显示的中文名称。</param>
public sealed record ThemeOption(AppTheme Value, string DisplayName);
