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
            services.AddSingleton<IThemeService, ThemeService>();

            // ViewModels
            services.AddSingleton<MainWindowViewModel>();

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
