using System;
using System.IO;
using System.Text.Json;
using WINUI.Models;

namespace WINUI.Services;

/// <summary>
/// 基于本地 JSON 文件的设置服务。
/// 文件位于 <c>%LOCALAPPDATA%\&lt;AppDataFolderName&gt;\settings.json</c>。
/// </summary>
public sealed class JsonSettingsService : ISettingsService
{
    /// <summary>应用数据目录名（对应产品名 Minecraft Fluent Launcher）。</summary>
    private const string AppDataFolderName = "MinecraftFluentLauncher";

    private const string FileName = "settings.json";

    private readonly string _filePath;

    public JsonSettingsService()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppDataFolderName);

        _filePath = Path.Combine(directory, FileName);
        Settings = Load();
    }

    /// <inheritdoc />
    public AppSettings Settings { get; }

    /// <inheritdoc />
    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Settings, SettingsJsonContext.Default.AppSettings);
            File.WriteAllText(_filePath, json);
        }
        catch (IOException)
        {
            // 磁盘/文件被占用等 IO 问题：不阻断主流程，后续接入日志后记录。
        }
        catch (UnauthorizedAccessException)
        {
            // 无写入权限：不阻断主流程，后续接入日志后记录。
        }
    }

    private AppSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize(json, SettingsJsonContext.Default.AppSettings)
                   ?? new AppSettings();
        }
        catch (IOException)
        {
            // 读取失败：回退到默认设置。
            return new AppSettings();
        }
        catch (JsonException)
        {
            // 文件损坏或格式不合法：回退到默认设置。
            return new AppSettings();
        }
    }
}
