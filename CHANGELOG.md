# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **大更新 ⑤-3｜破坏性操作二次确认与结果通知**：不可撤销的操作统一改为弹出 `ContentDialog` 二次确认，操作结果同时以 `InfoBar` 通知反馈。
  - **删除实例**（实例页）：确认框说明「存档、模组与配置都会被移除，且无法撤销」，满足后才执行；取消时状态栏给出提示。
  - **删除离线账户**（账户页）：同样先确认再执行，取消不改动任何数据。
  - **恢复默认设置**（设置页）：说明会覆盖主题、语言、界面动画、内存、下载源与日志级别等全部设置项，并注明「账户与实例数据不受影响」。
  - **清空下载队列**（下载中心）：队列为空时直接提示、无需确认；下载进行中时拒绝并给出警告通知；存在未完成任务时在确认框内明确告知将丢弃多少个任务。
  - 上述确认框的确认按钮标红（`AppDangerButtonStyle`）且默认焦点落在「取消」，避免误触回车导致数据丢失；多个确认框请求经信号量串行化。
  - 补充轻量通知：创建实例、启动未安装的实例（警告）、切换 / 注销账户、添加离线账户（含名称空、超长、重名三类校验失败提示）、全部启用 / 全部禁用模组（并说明影响数量）、检查更新结果、下载入队（含重复入队警告）、队列完成与清空成功。
  - 涉及视图模型：`InstancesPageViewModel`、`AccountsPageViewModel`、`SettingsPageViewModel`、`DownloadsPageViewModel`、`ModsPageViewModel` 均改为通过 `IInteractionService` 执行确认与通知，不再直接修改状态栏文案作为唯一反馈。
  - 页面事件处理器（删除实例、删除离线账户）改为 `async void` 以等待确认结果。
- **大更新 ⑤-2｜列表页接入加载 / 空 / 错误三态**：下载中心、实例、模组、版本详情四个页面统一改用状态容器，不再各写一套空状态。
  - 新增视图模型基类 `ViewModels/PageViewModelBase`：持有 `State`（`Content` / `Loading` / `Empty` / `Error`）与 `ErrorMessage`，提供 `RunLoadAsync`（加载期间进入加载态，异常时进入错误态并记录原因，**不向外抛出**）与 `UpdateContentState`（按是否有可见项收敛为空态或内容态；加载中与错误态不被覆盖，避免空状态闪烁、错误提示被筛选操作顶掉）。
  - 四个视图模型改为继承该基类，并各自覆写空状态文案：区分「尚无数据」与「没有匹配结果」两种情形，图标也随之切换（无数据用 Package，无匹配用放大镜）。
  - 各页的加载动作重构为可重复执行的命令（下载 / 实例 `ReloadCommand`、模组 / 版本详情 `ReloadCommand`），既是首次加载入口，也是错误状态下「重试」按钮与页头「刷新」的实现；下载中心的「刷新」由纯文案演示改为真实重新取数。
  - 版本详情页在拿不到导航参数（版本信息缺失）时进入错误态，此时「重试」按钮自动变为「返回下载中心」，避免反复重试同一个无效请求。
  - `MockLauncherDataService` 增加**模拟耗时**，使加载态可见；并提供仅通过环境变量生效的演示开关：`MFL_MOCK_DELAY`（每次查询的模拟耗时，默认 200 毫秒，设为 0 可关闭）、`MFL_MOCK_FAIL`（让指定查询抛错，用于验证错误态与「重试」，可用键 `versions` / `instances` / `downloads` / `loaders` / `mods` / `news` / `account` / `offline` / `logs`）。两个开关不影响正常使用，数据仍为纯 Mock、无网络请求。
- **大更新 ⑤-1｜状态与通知基础设施**：为后续所有页面统一「加载 / 空 / 错误」三态与「对话框 / 通知」反馈，不再由各页面各自拼装。
  - 新增模板化控件 `Controls/StatePanel`（样式 `Themes/StatePanel.xaml`）：在正常内容之上叠放加载层、空状态层、错误层，通过 `VisualStateManager` 切换可见性；暴露 `State` / `LoadingText` / `EmptyGlyph` / `EmptyTitle` / `EmptyText` / `ErrorGlyph` / `ErrorTitle` / `ErrorText` / `RetryText` / `RetryCommand` 等属性。错误层仅在提供 `RetryCommand` 时显示「重试」按钮。
  - 新增 `Models/PageState`（`Content` / `Loading` / `Empty` / `Error`）。
  - 新增服务 `Services/IInteractionService` / `Services/InteractionService`：`ConfirmAsync`（ContentDialog 二次确认，破坏性操作默认焦点落在「取消」且确认按钮标红）、`AlertAsync`（单按钮提示）、`Notify`（发出轻量通知）。多个对话框请求经信号量串行化，避免 WinUI 只允许单个 ContentDialog 的限制。
  - 新增通知宿主 `Controls/NotificationHost`：订阅交互服务的通知事件，在窗口右下角渲染一叠 `InfoBar`，失败 / 警告停留更久，可手动关闭，自动消失后延迟移除以保留收起动画。宿主不绘制背景，空白区域不拦截鼠标事件。
  - 新增 `Models/NotificationSeverity`、`Models/AppNotification`、`ViewModels/NotificationItemViewModel`。
  - 新增破坏性操作按钮样式 `AppDangerButtonStyle`；`Themes/Icons.xaml` 增补关闭、空状态、无结果、成功、警告、错误共 6 个字形；`Themes/Tokens.xaml` 增补通知间距令牌。
  - `MainWindow` 承载通知宿主并在内容进入可视树后把 `XamlRoot` 交给交互服务。
- **大更新 ④-8｜下载中心扩展（分页 · 七类分类 · 版本详情）**：`Views/DownloadsPage` 增强，并新增二级页面 `Views/VersionDetailPage`。
  - 分类调整为 **版本 / 模组 / 资源包 / 光影 / 世界 / 数据包 / 整合包**（共 7 类）：移除独立的「模组加载器」分类（改为随版本安装）、「地图存档」更名「世界」、新增「数据包」。
  - 列表**分页**：数字页码（围绕当前页最多显示 7 个）+ 首页 / 上一页 / 下一页 / 末页；每页条数可选 10 / 20 / 50；筛选或搜索变化时自动回到第一页。
  - **版本详情页**：由列表中的「查看」进入（路由键 `version-detail`）。展示版本号、发布通道、发布日期、体积、下载次数与简介；**多选**模组加载器（Fabric / NeoForge / Forge / Quilt）并选择加载器版本后「一起安装」，右侧实时显示安装摘要与目标实例名。可选加载器随游戏版本变化（快照 / 远古版无第三方加载器；Forge 面向 1.20.x 及以下，NeoForge 面向 1.20.1 及以上）。
  - 「一起安装」为纯 UI 演示：以三阶段进度模拟，**不下载任何文件、不创建真实实例**。
  - 非「版本」条目仍为「下载」按钮，直接加入下载队列。
- 新增模型 `Models/ModLoader`、`Models/LoaderEntry` 及 `Models/ModLoaderExtensions`；`Models/DownloadItem` 增加发布通道、发布日期与「是否为版本条目」等属性；`Models/DownloadCategory` 调整为 7 类。
- 新增视图模型 `ViewModels/VersionDetailPageViewModel`（含多选加载器与安装摘要）、`ViewModels/LoaderOptionViewModel`、`ViewModels/PageButtonViewModel`。
- `ILauncherDataService` 增加 `GetLoadersAsync`；`MockLauncherDataService` 下载条目扩充到 37 条，并按游戏版本返回可用加载器。
- 导航服务 `INavigationService` / `NavigationService` 支持**携带参数导航**与**回退**（`Navigate(key, parameter)` / `GoBack` / `CanGoBack`），并新增 `version-detail` 路由；返回下载中心时保留分页与筛选状态。
- `Themes/Icons.xaml` 新增返回与右向箭头字形（`IconBackGlyph` / `IconChevronRightGlyph`）。- **大更新 ④-7｜日志页与关于页**：`Views/LogsPage`、`Views/AboutPage` 由占位页重做。
  - 日志页：级别门槛筛选（全部 / 调试及以上 / 常规及以上 / 警告及以上 / 仅错误）、关键字搜索、自动滚动开关；固定列宽的时间 / 级别徽章 / 来源 / 消息四列布局，错误与警告带颜色指示点；支持复制当前日志（**真实剪贴板**）、清空视图与重新载入。
  - 关于页：产品卡（图标、版本、阶段、版权、简介）、技术栈卡、当前阶段说明卡；侧栏含运行环境（操作系统 / .NET 运行时 / 进程架构 / 数据目录，均为真实取值）与「复制环境信息」、链接卡与致谢卡。
- 新增 `Models/AppInfo`（集中定义产品名、版本、技术栈与运行环境描述）与 `Models/LogEntry`；`AppLogLevelExtensions` 增加短标签。
- 新增服务 `Services/IClipboardService` / `Services/ClipboardService`（封装系统剪贴板，失败时返回 `false` 而不抛出）。
- 新增视图模型 `ViewModels/LogsPageViewModel`、`ViewModels/AboutPageViewModel`；`ILauncherDataService` 增加 `GetLogEntriesAsync`。
- `MainWindowViewModel` 的标题改为引用 `AppInfo.DisplayName`。
- **大更新 ④-6｜设置页**：`Views/SettingsPage` 由占位页重做为设置中心，**接入真实持久化**。
  - 分组：外观（主题 / 界面语言 / 界面动画 / 显示快照版本）、启动器行为（启动时检查更新 / 启动游戏后关闭启动器）、Java 运行时（自动检测 / 指定路径 / 默认最小与最大内存）、下载（下载源 / 最大并发数）、诊断（日志级别）、数据（数据目录、打开目录、恢复默认设置）。
  - 所有开关与下拉改动都会立即写入 `%LOCALAPPDATA%\MinecraftFluentLauncher\settings.json` 并在下次启动恢复；最大内存自动不小于最小内存。
  - 仅「浏览 Java 路径」「打开数据目录」为演示行为。
- 扩展 `Models/AppSettings`：新增语言、动画、快照显示、更新检查、关闭策略、Java 检测与路径、内存上下限、下载源与并发数、日志级别等字段（均带默认值，旧设置文件可正常反序列化）。
- 新增模型 `Models/AppLogLevel` 及标签扩展；`ISettingsService` 增加 `DataDirectory`，`JsonSettingsService` 实现之。
- 新增视图模型 `ViewModels/SettingsPageViewModel`。
- **大更新 ④-5｜账户页**：`Views/AccountsPage` 由占位页重做为账户管理页（Mock 数据）。
  - 微软账户卡片：登录状态徽章、登录 / 注销按钮（按状态互斥显示）、登录进度条；登录流程以 3 段文案模拟授权 → 获取 Xbox Live 凭据 → 获取 Minecraft 档案，**不打开浏览器、不保存凭据**。
  - 离线账户卡片：名称输入（限 16 字符、去重、空值校验）+ 账户列表（头像、最近使用与创建时间、「使用 / 删除」操作），并含空状态提示。
  - 侧栏：当前账户卡片（头像、名称、类型、本地实例数与离线账户数）与「须知」说明卡片。
- 新增模型 `Models/OfflineAccount`；新增视图模型 `ViewModels/AccountsPageViewModel`；`ILauncherDataService` 增加 `GetOfflineAccountsAsync`。
- **大更新 ④-4｜模组管理页**：`Views/ModsPage` 由占位页重做为模组管理器（Mock 数据）。
  - 工具栏：实例选择、搜索（名称 / 作者 / 简介）、筛选（全部 / 已启用 / 已禁用 / 可更新）。
  - 主体：`ListView` 模组列表，每行含名称、版本、运行侧与依赖概览、作者 / 加载器 / 体积，以及启用开关（TwoWay）；切换开关会即时更新统计，并在「已启用 / 已禁用」筛选下同步刷新列表。
  - 侧栏：模组详情面板（作者、版本、运行侧、体积、简介、适配、依赖、文件名与更新入口），未选中时显示引导提示。
  - 页头提供「检查更新 / 打开目录」，底部提供「全部启用 / 全部禁用」与统计摘要。全部操作均为演示，**不改动任何 .jar 文件**。
- 新增模型 `Models/ModEntry`、`Models/ModFilter`、`Models/ModSide` 及标签扩展；新增 `ViewModels/ModItemViewModel`。
- 新增视图模型 `ViewModels/ModsPageViewModel`；`ILauncherDataService` 增加 `GetModsAsync`。
- **大更新 ④-3｜下载中心页**：`Views/DownloadsPage` 由占位页重做为下载中心（Mock 数据）。
  - 工具栏：搜索（名称 / 作者 / 简介）、分类（全部 / 游戏版本 / 加载器 / 模组 / 资源包 / 光影 / 整合包 / 地图存档）、下载源（官方 / BMCLAPI / 社区镜像）与刷新。
  - 主体：`ItemsRepeater` + `StackLayout` 条目列表，含名称、分类与安装状态徽章、简介、作者 / 版本 / 体积 / 下载次数，以及「下载」按钮。
  - 侧栏：下载队列卡片，含逐条进度、整体队列进度、队列摘要与「开始下载 / 清空」；下载为定时递增的进度演示，**不产生网络请求、不写磁盘**。
- 新增模型 `Models/DownloadItem`、`Models/DownloadCategory`、`Models/DownloadSource` 及对应标签扩展；新增 `ViewModels/DownloadTaskViewModel`。
- 新增视图模型 `ViewModels/DownloadsPageViewModel`；`ILauncherDataService` 增加 `GetDownloadItemsAsync`。
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
