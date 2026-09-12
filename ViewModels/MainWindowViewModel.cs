using CommunityToolkit.Mvvm.ComponentModel;

namespace WINUI.ViewModels;

/// <summary>
/// 主窗口视图模型。
/// 当前仅承载窗口标题与状态文案，用于打通「DI 注入 → View 绑定」链路。
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    /// <summary>应用标题。占位名，待产品定名后统一替换。</summary>
    [ObservableProperty]
    public partial string AppTitle { get; set; }

    /// <summary>启动器状态文案。</summary>
    [ObservableProperty]
    public partial string StatusText { get; set; }

    public MainWindowViewModel()
    {
        AppTitle = "Minecraft 启动器";
        StatusText = "项目骨架已就绪";
    }
}
