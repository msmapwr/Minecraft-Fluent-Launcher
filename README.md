# Minecraft 启动器

[![CI](https://github.com/msmapwr/Minecraft-Fluent-Launcher/actions/workflows/ci.yml/badge.svg)](https://github.com/msmapwr/Minecraft-Fluent-Launcher/actions/workflows/ci.yml)

基于 **WinUI 3（Windows App SDK）** 开发的 Minecraft 启动器桌面应用。当前处于 **UI 优先开发阶段**，尚未接入真实启动核心。

> 项目名与产品名暂为占位，待定名后统一替换。
>
> 本项目的开发流程受 [AGENTS.md](./AGENTS.md) 约束。

---

## 当前状态

| 项 | 值 |
|---|---|
| 阶段 | 第 1 阶段：UI（超大更新进行中） |
| 发布版本 | 尚未发布（见 [CHANGELOG](./CHANGELOG.md)） |
| 构建状态 | ✅ 通过（0 警告 0 错误） |
| 产品名 | Minecraft Fluent Launcher（MFL） |
| 应用图标 | `Assets/App.jpg` → 透明多尺寸 PNG + `.ico` |

---

## 环境要求

| 组件 | 要求 |
|---|---|
| 操作系统 | Windows 10 1809（10.0.17763）及以上 |
| .NET SDK | **9.0 或更高（推荐 10）** — partial property 所需 |
| Windows App SDK | 2.4.0（NuGet 自动还原） |
| 目标框架 | `net8.0-windows10.0.19041.0` |
| 打包方式 | 单项目 MSIX（同时支持非打包运行） |

> `LangVersion` 为 `latest`，用于启用 C# partial property，以配合 CommunityToolkit.Mvvm 生成 WinRT/AOT 友好的可观察属性。因此构建机需要较新的 .NET SDK。

---

## 构建与运行

使用 .NET CLI 构建（注意需指定 `Platform`，项目仅支持 x86/x64/ARM64）：

```bash
# 构建（x64 Debug）
dotnet build WINUI.csproj -c Debug -p:Platform=x64

# 运行（非打包模式）
dotnet run --project WINUI.csproj -p:Platform=x64
```

也可以直接使用 Visual Studio 2022 打开 `WINUI.slnx`。

---

## 技术栈

| 领域 | 选型 |
|---|---|
| UI 框架 | WinUI 3 / Windows App SDK 2.4.0 |
| 语言 | C#（目标框架 .NET 8） |
| 架构模式 | MVVM |
| MVVM 库 | CommunityToolkit.Mvvm 8.4.2 |
| 依赖注入 | Microsoft.Extensions.DependencyInjection 10.0.12 |
| 视觉风格 | Windows 原生 Fluent（Mica 云母材质） |

---

## 目录结构

```text
WINUI/
├─ App.xaml / App.xaml.cs   # 应用入口：构建 DI 容器、启动主窗口
├─ Views/                   # 视图（主窗口外壳 + 8 个页面）
├─ ViewModels/              # 视图模型
├─ Models/                  # 数据模型（枚举 / 设置 / 页面数据）
├─ Services/                # 服务层（设置 / 主题 / 导航）
├─ Controls/                # 自定义控件（后续阶段）
├─ Themes/                  # 设计令牌、控件样式、图标字形
├─ Assets/                  # 图标、启动画面等资源
└─ docs/                    # 项目文档
```

---

## 文档

- [开发计划（docs/PLAN.md）](docs/PLAN.md)
- [变更日志（CHANGELOG.md）](CHANGELOG.md)
- [开发规范（AGENTS.md）](AGENTS.md)
