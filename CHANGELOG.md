# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **大更新 ④-2｜实例列表页**：`Views/InstancesPage` 由占位页重做为实例管理页（Mock 数据）。
  - 工具栏：搜索（实例名 / 版本号 / 加载器）、筛选（全部 / 已安装 / 未安装）、排序（最近游玩 / 名称 / 占用空间），三者均在内存中即时生效。
  - 主体：`ItemsRepeater` + `UniformGridLayout` 响应式卡片网格；每张卡片含实例图标、名称、最近游玩时间、版本与加载器、通道 / 安装状态 / 占用空间徽章、累计时长，以及「启动 / 目录 / 删除」操作。
  - 空状态区分「尚无实例」与「无匹配结果」两种文案；底部状态栏显示实时操作反馈与总数摘要。
- 新增模型 `Models/GameInstance`、`Models/InstanceFilter`、`Models/InstanceSort`、`Models/SelectOption<T>`、`Models/VersionChannelExtensions`。
- 新增视图模型 `ViewModels/InstancesPageViewModel`；`ILauncherDataService` 增加 `GetInstancesAsync`。
- **大更新 ④-1｜启动页**：`Views/LaunchPage` 由占位页重做为完整的启动控制台（Mock 数据）。
  - 左列：实例卡片（版本/加载器下拉、加载器/通道/安装状态徽章、发布日期、安装目录）、启动卡片（强调色大按钮 + 分阶段进度条 + 状态行）、运行配置卡片（内存分配滑杆、Java 运行时）。
  - 右列：账户卡片（`PersonPicture` 头像 + 玩家名 + 账户类型）、新闻与公告卡片。
  - 「启动游戏」为纯 UI 演示：模拟「校验文件 → 准备 Java 运行时 → 启动」三阶段，**不拉起任何 Java 进程**；运行中按钮切换为「结束游戏」。
- 新增数据层接缝 `Services/ILauncherDataService`，以及 Mock 实现 `Services/MockLauncherDataService`（硬编码数据，无网络请求）；接入真实启动核心时仅需替换 DI 注册。
- 新增模型 `Models/GameVersion`、`Models/NewsItem`、`Models/PlayerAccount`、`Models/VersionChannel`。
- 新增视图模型 `ViewModels/LaunchPageViewModel`。
- `Themes/Controls.xaml` 新增徽章体系：`AppBadgeStyle` / `AppBadgeAccentStyle` 与对应文字样式 `AppBadgeTextStyle` / `AppBadgeAccentTextStyle`。
- `App` 新增 `GetService<T>()` 静态助手，供由 `Frame.Navigate(Type)` 反射创建、无法构造函数注入的页面解析视图模型。
- **产品定名与品牌化**：产品正式命名为 **Minecraft Fluent Launcher（MFL）**。
  - 应用图标取自 `Assets/App.jpg`（像素风金苹果），程序化去白底后生成全套带透明通道的多尺寸资源：`Square44x44Logo`、`Square150x150Logo`、`StoreLogo`、`LockScreenLogo`、`Wide310x150Logo`、`SplashScreen`，以及多尺寸 `App.ico`，另新增应用内使用、无缩放限定符的 `Assets/AppIcon.png`。
  - `WINUI.csproj` 新增 `<AssemblyName>MinecraftFluentLauncher</AssemblyName>` 与 `<ApplicationIcon>Assets\App.ico</ApplicationIcon>`（`RootNamespace` 保持 `WINUI` 不变，避免无收益的全量改名）。
  - `Package.appxmanifest` 的 `DisplayName` / `Description`、`app.manifest` 的 `assemblyIdentity`、`Views/MainWindow.xaml` 的窗口 `Title` 均切换为正式产品名。
  - 自定义标题栏的应用图标由字形图标改为真实应用图标（`Assets/AppIcon.png`）。
  - 应用数据目录由 `%LOCALAPPDATA%\MinecraftLauncher` 改为 `%LOCALAPPDATA%\MinecraftFluentLauncher`。
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
- 新增应用外壳：主窗口改为 `NavigationView` + 内容 `Frame`，侧边栏提供启动 / 实例 / 下载 / 模组 / 账户 / 设置 / 日志 / 关于八个入口。
- 新增导航服务 `INavigationService` / `NavigationService`（路由键 → 页面类型，未知键安全返回）。
- 新增 8 个页面占位：`Views/LaunchPage`、`InstancesPage`、`DownloadsPage`、`ModsPage`、`AccountsPage`、`SettingsPage`、`LogsPage`、`AboutPage`。
- 新增自定义标题栏：内容延伸进标题栏区域，窗口按钮背景透明以露出 Mica 材质，标题栏显示应用图标与名称。

### Changed

- 主窗口不再是设计系统预览面，改为承载导航外壳；主题切换入口移入侧边栏底部。

- `WINUI.csproj` 的 `LangVersion` 设为 `latest`，以启用 C# partial property（避免 `MVVMTK0045` 警告）。
