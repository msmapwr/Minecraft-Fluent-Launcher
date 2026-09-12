namespace WINUI.Services;

/// <summary>系统剪贴板访问。</summary>
public interface IClipboardService
{
    /// <summary>把文本写入系统剪贴板。</summary>
    /// <returns>写入成功返回 <c>true</c>；剪贴板被其他进程占用等情况下返回 <c>false</c>。</returns>
    bool TrySetText(string text);
}
