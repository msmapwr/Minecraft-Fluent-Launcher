using WINUI.Models;

namespace WINUI.ViewModels;

/// <summary>
/// 安装确认对话框中一个可选加载器的展示项。
/// </summary>
/// <param name="DisplayName">展示名（如 <c>Fabric</c>）。</param>
/// <param name="Detail">说明（版本等信息）。</param>
/// <param name="Kind">加载器种类；<c>null</c> 表示原版（Vanilla）。</param>
/// <param name="Version">将安装的加载器版本。</param>
public sealed record LoaderChoiceViewModel(
    string DisplayName,
    string Detail,
    ModLoader? Kind = null,
    string? Version = null);
