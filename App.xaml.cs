using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using WINUI.Services;
using WINUI.ViewModels;
using WINUI.Views;

namespace WINUI
{
    /// <summary>
    /// 应用程序入口，负责构建 DI 容器并启动主窗口。
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// 全局服务容器。优先使用构造函数注入；此处仅为无法注入的场景提供兜底访问。
        /// </summary>
        public static IServiceProvider Services { get; private set; } = null!;

        /// <summary>
        /// 从容器解析服务。
        /// <para>
        /// <b>仅供无法使用构造函数注入的场景</b>（例如由 <c>Frame.Navigate(Type)</c> 反射创建的页面）。
        /// 其余情况一律使用构造函数注入。
        /// </para>
        /// </summary>
        public static T GetService<T>() where T : notnull => Services.GetRequiredService<T>();

        public App()
        {
            InitializeComponent();
            Services = ConfigureServices();
        }

        /// <summary>注册应用所需的视图模型与窗口。</summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<ISettingsService, JsonSettingsService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<IInteractionService, InteractionService>();
            services.AddSingleton<IAnimationService, AnimationService>();

            // 数据来源：⑦-1 起版本清单走 CMLLib 真实实现，其余查询暂由 Mock 承载（随 ⑦-2~⑦-7 真实化）。
            services.AddSingleton<MockLauncherDataService>();
            services.AddSingleton<ILauncherDataService, CoreLauncherDataService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LaunchPageViewModel>();
            services.AddSingleton<InstancesPageViewModel>();
            services.AddSingleton<DownloadsPageViewModel>();
            services.AddSingleton<VersionDetailPageViewModel>();
            services.AddSingleton<ModsPageViewModel>();
            services.AddSingleton<AccountsPageViewModel>();
            services.AddSingleton<SettingsPageViewModel>();
            services.AddSingleton<LogsPageViewModel>();
            services.AddSingleton<AboutPageViewModel>();

            // Views
            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = Services.GetRequiredService<MainWindow>();
            _window.Activate();
        }
    }
}
