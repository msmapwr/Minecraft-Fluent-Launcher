using System;
using Windows.ApplicationModel.DataTransfer;

namespace WINUI.Services;

/// <inheritdoc cref="IClipboardService" />
public sealed class ClipboardService : IClipboardService
{
    /// <inheritdoc />
    public bool TrySetText(string text)
    {
        try
        {
            var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            package.SetText(text);
            Clipboard.SetContent(package);
            return true;
        }
        catch (Exception)
        {
            // 剪贴板可能被其他进程占用（外部程序正在读写），此类异常的具体类型随
            // 宿主环境而异，这里统一视为「写入失败」并交由调用方提示，不影响主流程。
            return false;
        }
    }
}
