using WINUI.Models;

namespace WINUI.Services;

/// <summary>应用设置的读取与保存。</summary>
public interface ISettingsService
{
    /// <summary>
    /// 当前设置实例。直接修改其属性后调用 <see cref="Save"/> 即可落盘。
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>把当前设置写入本地文件。</summary>
    void Save();
}
