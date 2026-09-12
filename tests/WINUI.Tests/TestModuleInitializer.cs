using System;
using System.Runtime.CompilerServices;

namespace WINUI.Tests;

/// <summary>
/// 模块初始化：在任何被测类型静态字段求值之前固定 Mock 行为开关。
/// <para>
/// <see cref="Services.MockLauncherDataService"/> 的模拟耗时 / 故障注入
/// 在类型初始化时读取环境变量；测试进程统一关闭延迟、清掉故障注入，
/// 保证测试速度且不受本机遗留环境变量影响。
/// </para>
/// </summary>
internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Environment.SetEnvironmentVariable("MFL_MOCK_DELAY", "0");
        Environment.SetEnvironmentVariable("MFL_MOCK_FAIL", null);
    }
}
