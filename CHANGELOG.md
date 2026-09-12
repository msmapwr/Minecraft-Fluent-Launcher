# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- 搭建 MVVM 与依赖注入基础：引入 `CommunityToolkit.Mvvm` 8.4.2 与 `Microsoft.Extensions.DependencyInjection` 10.0.12，`App` 负责构建 DI 容器并从容器解析主窗口。
- 新增 `Views/`、`ViewModels/` 目录结构；主窗口迁移至 `Views/MainWindow`，改为构造函数注入视图模型。
- 新增 `ViewModels/MainWindowViewModel`，打通「DI 注入 → View 绑定」链路。
- 新增 `.gitignore` 与 `.gitattributes`（.NET / Visual Studio / WinUI 3 忽略规则、统一换行）。
- 新增 `README.md`、`docs/PLAN.md` 项目文档，并初始化本 CHANGELOG。
- 初始化 Git 仓库，纳入 WinUI 3 项目基线（Windows App SDK 2.4.0、单项目 MSIX、Mica 背景）。
- 新增 `Themes/Tokens.xaml` 设计令牌：间距、内边距、圆角、字号、字体，以及深色 / 浅色 / 高对比度三套主题画笔；`App.xaml` 已合并该字典，主窗口改用令牌而非硬编码数值。
- 新增主题服务 `IThemeService` / `ThemeService`：支持运行时切换「跟随系统 / 浅色 / 深色」，主窗口注入该服务并提供切换入口。
- 新增设置持久化：`ISettingsService` / `JsonSettingsService` 把设置写入 `%LOCALAPPDATA%\MinecraftLauncher\settings.json`，使用源生成 JSON 上下文以保证裁剪安全；主题偏好在下次启动时恢复。
- 新增基础控件样式 `Themes/Controls.xaml`：页面标题 / 小节标题 / 正文 / 说明四档文本样式、卡片样式，以及强调色主按钮样式（含悬停、按下、禁用状态）；主窗口改为设计系统预览面。
- 新增图标体系 `Themes/Icons.xaml`：统一使用 Segoe Fluent Icons（回退 Segoe MDL2 Assets），集中定义导航 / 操作 / 信息类字形，并提供 `AppIconStyle`。

### Changed

- `WINUI.csproj` 的 `LangVersion` 设为 `latest`，以启用 C# partial property（避免 `MVVMTK0045` 警告）。
