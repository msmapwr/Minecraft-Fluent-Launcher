using System.Text.Json.Serialization;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 设置 JSON 序列化上下文（源生成）。
/// 使用源生成而非反射，以保证在裁剪（PublishTrimmed）场景下仍可正常工作。
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext
{
}
