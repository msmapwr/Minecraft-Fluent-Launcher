using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WINUI.Models;
using WINUI.Services;

namespace WINUI.ViewModels;

/// <summary>
/// 账户页的视图模型。
/// <para>
/// 微软正版登录与离线账户管理均为 <b>UI 演示</b>：不会打开浏览器、不会发起 OAuth 请求，
/// 也不会写入任何凭据。
/// </para>
/// </summary>
public sealed partial class AccountsPageViewModel : ObservableObject
{
    private readonly ILauncherDataService _dataService;

    /// <summary>当前账户。</summary>
    [ObservableProperty]
    public partial PlayerAccount? CurrentAccount { get; set; }

    /// <summary>微软账户是否已登录。</summary>
    [ObservableProperty]
    public partial bool IsMicrosoftSignedIn { get; set; }

    /// <summary>是否正在执行登录流程。</summary>
    [ObservableProperty]
    public partial bool IsSigningIn { get; set; }

    /// <summary>新建离线账户的名称输入。</summary>
    [ObservableProperty]
    public partial string NewOfflineName { get; set; }

    /// <summary>已加载的实例数量（用于账户概览）。</summary>
    [ObservableProperty]
    public partial int InstanceCount { get; set; }

    /// <summary>状态栏文案。</summary>
    [ObservableProperty]
    public partial string StatusMessage { get; set; }

    /// <summary>离线账户列表。</summary>
    public ObservableCollection<OfflineAccount> OfflineAccounts { get; } = [];

    /// <summary>是否已登录微软账户。</summary>
    public bool IsMicrosoftSignedOut => !IsMicrosoftSignedIn;

    /// <summary>登录按钮是否可用。</summary>
    public bool IsSignInEnabled => !IsSigningIn;

    /// <summary>离线账户列表是否为空。</summary>
    public bool IsOfflineListEmpty => OfflineAccounts.Count == 0;

    /// <summary>当前账户标签。</summary>
    public string CurrentAccountName => CurrentAccount?.Name ?? "未登录";

    /// <summary>当前账户类型标签。</summary>
    public string CurrentAccountType => CurrentAccount?.AccountType ?? "—";

    /// <summary>当前账户头像文字。</summary>
    public string CurrentAccountInitials => CurrentAccount?.Initials ?? "?";

    /// <summary>微软账户登录状态标签。</summary>
    public string MicrosoftStateLabel => IsMicrosoftSignedIn ? "已登录" : "未登录";

    /// <summary>实例数量标签。</summary>
    public string InstanceCountLabel => $"{InstanceCount} 个";

    /// <summary>离线账户数量标签。</summary>
    public string OfflineAccountCountLabel => $"{OfflineAccounts.Count} 个";

    /// <summary>破坏性操作的二次确认与操作结果通知。</summary>
    private readonly IInteractionService _interaction;

    public AccountsPageViewModel(ILauncherDataService dataService, IInteractionService interaction)
    {
        _dataService = dataService;
        _interaction = interaction;

        NewOfflineName = string.Empty;
        StatusMessage = "准备就绪";

        OfflineAccounts.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsOfflineListEmpty));
            OnPropertyChanged(nameof(OfflineAccountCountLabel));
        };

        // Mock 实现返回已完成的 Task，此处会同步跑完。
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        CurrentAccount = await _dataService.GetCurrentAccountAsync();
        IsMicrosoftSignedIn = CurrentAccount.IsSignedIn;

        foreach (var account in await _dataService.GetOfflineAccountsAsync())
        {
            OfflineAccounts.Add(account);
        }

        InstanceCount = (await _dataService.GetInstancesAsync()).Count;
    }

    partial void OnCurrentAccountChanged(PlayerAccount? value)
    {
        OnPropertyChanged(nameof(CurrentAccountName));
        OnPropertyChanged(nameof(CurrentAccountType));
        OnPropertyChanged(nameof(CurrentAccountInitials));
    }

    partial void OnIsMicrosoftSignedInChanged(bool value)
    {
        OnPropertyChanged(nameof(IsMicrosoftSignedOut));
        OnPropertyChanged(nameof(IsSignInEnabled));
        OnPropertyChanged(nameof(MicrosoftStateLabel));
    }

    partial void OnIsSigningInChanged(bool value) => OnPropertyChanged(nameof(IsSignInEnabled));

    partial void OnInstanceCountChanged(int value) => OnPropertyChanged(nameof(InstanceCountLabel));

    /// <summary>使用微软账户登录（演示）。</summary>
    [RelayCommand]
    private async Task SignInMicrosoftAsync()
    {
        if (IsSigningIn || IsMicrosoftSignedIn)
        {
            return;
        }

        IsSigningIn = true;
        StatusMessage = "正在打开授权页面…（演示，不会真的打开浏览器）";

        for (var step = 1; step <= 6; step++)
        {
            await Task.Delay(110);
            StatusMessage = step switch
            {
                <= 2 => "正在打开授权页面…（演示）",
                <= 4 => "正在获取 Xbox Live 凭据…（演示）",
                _ => "正在获取 Minecraft 档案…（演示）",
            };
        }

        IsSigningIn = false;
        IsMicrosoftSignedIn = true;
        CurrentAccount = new PlayerAccount
        {
            Name = "Msmapwr",
            AccountType = "微软正版",
            IsSignedIn = true,
        };

        StatusMessage = "已登录微软账户（演示，未保存任何凭据）";
    }

    /// <summary>注销微软账户（演示）。</summary>
    [RelayCommand]
    private void SignOutMicrosoft()
    {
        if (!IsMicrosoftSignedIn)
        {
            return;
        }

        IsMicrosoftSignedIn = false;
        CurrentAccount = null;
        StatusMessage = "已注销微软账户（演示）";

        _interaction.Notify("已注销微软账户（演示）", NotificationSeverity.Informational, "已注销");
    }

    /// <summary>添加离线账户（演示）。</summary>
    [RelayCommand]
    private void AddOfflineAccount()
    {
        var name = NewOfflineName.Trim();

        if (name.Length == 0)
        {
            StatusMessage = "请输入离线账户名称";
            _interaction.Notify("请先输入离线账户名称。", NotificationSeverity.Warning, "无法添加");
            return;
        }

        if (name.Length > 16)
        {
            StatusMessage = "名称过长：Minecraft 玩家名最多 16 个字符";
            _interaction.Notify("Minecraft 玩家名最多 16 个字符。", NotificationSeverity.Warning, "无法添加");
            return;
        }

        if (OfflineAccounts.Any(account => string.Equals(account.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            StatusMessage = $"离线账户「{name}」已存在";
            _interaction.Notify($"离线账户「{name}」已经存在了。", NotificationSeverity.Warning, "无法添加");
            return;
        }

        OfflineAccounts.Add(new OfflineAccount
        {
            Name = name,
            CreatedAt = DateTimeOffset.Now,
        });

        NewOfflineName = string.Empty;
        StatusMessage = $"已添加离线账户「{name}」（演示，未写入磁盘）";

        _interaction.Notify(
            $"已添加离线账户「{name}」（演示，未写入磁盘）",
            NotificationSeverity.Success,
            "添加成功");
    }

    /// <summary>切换到指定离线账户（演示）。</summary>
    public void UseOfflineAccount(OfflineAccount account)
    {
        // OfflineAccount 保持不可变，这里用更新时间后的新实例替换原项。
        var index = OfflineAccounts.IndexOf(account);
        if (index < 0)
        {
            return;
        }

        var used = new OfflineAccount
        {
            Name = account.Name,
            CreatedAt = account.CreatedAt,
            LastUsedAt = DateTimeOffset.Now,
        };

        OfflineAccounts[index] = used;

        IsMicrosoftSignedIn = false;
        CurrentAccount = new PlayerAccount
        {
            Name = used.Name,
            AccountType = "离线账户",
            IsSignedIn = true,
        };

        StatusMessage = $"已切换到离线账户「{used.Name}」（演示）";

        _interaction.Notify(
            $"当前账户已切换为「{used.Name}」（演示）",
            NotificationSeverity.Informational,
            "已切换");
    }

    /// <summary>
    /// 删除离线账户。删除不可撤销，因此先弹出二次确认；
    /// 确认为演示行为：<b>不会删除磁盘上的任何数据</b>。
    /// </summary>
    /// <param name="account">目标离线账户。</param>
    public async Task RemoveOfflineAccountAsync(OfflineAccount account)
    {
        var confirmed = await _interaction.ConfirmAsync(
            "删除离线账户",
            $"确定要删除离线账户「{account.Name}」吗？\n删除后需要重新添加才能再次使用。",
            "删除",
            "取消");

        if (!confirmed)
        {
            StatusMessage = $"已取消删除「{account.Name}」";
            return;
        }

        if (!OfflineAccounts.Remove(account))
        {
            return;
        }

        if (CurrentAccount is not null && CurrentAccount.Name == account.Name)
        {
            CurrentAccount = null;
        }

        StatusMessage = $"已删除离线账户「{account.Name}」（演示，未删除磁盘数据）";

        _interaction.Notify(
            $"离线账户「{account.Name}」已删除（演示，未删除磁盘数据）",
            NotificationSeverity.Success,
            "删除成功");
    }
}
