# Minecraft Fluent Launcher

[![CI](https://github.com/msmapwr/Minecraft-Fluent-Launcher/actions/workflows/ci.yml/badge.svg)](https://github.com/msmapwr/Minecraft-Fluent-Launcher/actions/workflows/ci.yml)

一款基于 **WinUI 3 / Windows App SDK** 开发的 Minecraft 桌面启动器，主要面向 Windows 平台，使用 Windows Fluent 设计风格。
---

## 特点

### Windows Fluent 设计

项目使用 WinUI 3 构建界面，采用 Windows Fluent 设计，并使用 Mica 等 Windows 原生视觉效果。

### 操作简单

界面围绕 Minecraft 的安装、管理和启动流程设计，常用功能尽量保持清晰，减少不必要的操作。

### Minecraft 游戏管理

项目计划提供 Minecraft 版本、实例、Java、账户、Mod 和资源等功能，逐步完善启动器的日常使用场景。

### Windows 桌面应用

项目基于 WinUI 3 / Windows App SDK 开发，面向 Windows 桌面环境进行设计和优化。

---

## 下载

项目提供以下发行形式：

- **MSIX**：适合安装到 Windows
- **ZIP**：适合解压后直接使用

正式版本将在 GitHub Releases 中提供。

> 当前项目仍在开发中，部分功能尚未完成。

---

## 功能规划

Minecraft Fluent Launcher 计划支持以下功能：

- Minecraft 版本安装与管理
- 游戏实例管理
- Java 管理
- Microsoft / Minecraft 账户管理
- Forge、Fabric、NeoForge 等 Mod Loader
- Mod 管理
- Resource Pack 管理
- Shader 管理
- 游戏启动参数配置
- 游戏文件管理
- 下载与资源管理
- 游戏日志与运行状态
- 主题与界面设置

功能会根据项目开发进度逐步加入。

---

## 界面

项目使用 Windows Fluent 设计和 WinUI 3 构建界面。

截图和演示内容将在合适的时候加入。

---

## 技术栈

| 领域 | 选型 |
|---|---|
| UI 框架 | WinUI 3 |
| Windows 框架 | Windows App SDK |
| 语言 | C# |
| 目标框架 | .NET 8 |
| 架构模式 | MVVM |
| MVVM 库 | CommunityToolkit.Mvvm |
| 依赖注入 | Microsoft.Extensions.DependencyInjection |
| 视觉风格 | Windows Fluent / Mica |

---

## 开发环境

### 环境要求

| 组件 | 要求 |
|---|---|
| 操作系统 | Windows 10 1809（10.0.17763）及以上 |
| .NET SDK | 9.0 或更高，推荐 .NET 10 |
| Windows App SDK | 2.4.0 |
| 目标框架 | `net8.0-windows10.0.19041.0` |
| IDE | Visual Studio 2022（推荐） |

> 项目使用较新的 C# 语言特性，并依赖 CommunityToolkit.Mvvm 的代码生成功能，因此开发环境需要较新的 .NET SDK。

### 构建

使用 .NET CLI：

```bash
dotnet build WINUI.csproj -c Debug -p:Platform=x64
```

项目当前支持：

- x86
- x64
- ARM64

### 运行

```bash
dotnet run --project WINUI.csproj -p:Platform=x64
```

也可以直接使用 Visual Studio 2022 打开：

```text
WINUI.slnx
```

---

## 项目结构

```text
WINUI/
├─ App.xaml / App.xaml.cs   # 应用入口、依赖注入、主窗口启动
├─ Views/                   # 页面与视图
├─ ViewModels/              # ViewModel
├─ Models/                  # 数据模型
├─ Services/                # 服务层
├─ Controls/                # 自定义控件
├─ Themes/                  # 设计令牌、样式与图标资源
├─ Assets/                  # 应用资源
└─ docs/                    # 项目文档
```

---

## 参与贡献

欢迎提交 Issue 和 Pull Request。

可以参与：

- Bug 修复
- 功能开发
- UI / UX 改进
- 性能优化
- 文档完善
- 代码重构

较大的功能或架构调整，建议先通过 Issue 讨论方案，确认方向后再开始开发。

项目开发流程与 Agent 行为规范请参考：

- [AGENTS.md](./AGENTS.md)

---

## License

本项目采用 **MIT License** 开源。

详细内容请查看 [LICENSE](./LICENSE)。

---

## 项目文档

- [AGENTS.md](./AGENTS.md) — 开发规范
- [CHANGELOG.md](./CHANGELOG.md) — 变更日志
- [docs/PLAN.md](./docs/PLAN.md) — 开发计划

