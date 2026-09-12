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

### Changed

- `WINUI.csproj` 的 `LangVersion` 设为 `latest`，以启用 C# partial property（避免 `MVVMTK0045` 警告）。
