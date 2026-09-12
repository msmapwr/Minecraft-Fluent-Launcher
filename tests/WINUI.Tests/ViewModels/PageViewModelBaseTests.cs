using System;
using System.Threading.Tasks;
using WINUI.Models;
using WINUI.ViewModels;
using Xunit;

namespace WINUI.Tests.ViewModels;

/// <summary>
/// 页面四态基类（大更新 ⑤-2）的行为测试。
/// </summary>
public sealed class PageViewModelBaseTests
{
    /// <summary>暴露受保护成员的测试替身。</summary>
    private sealed class TestPageViewModel : PageViewModelBase
    {
        public new Task<bool> RunLoadAsync(Func<Task> load, string failurePrefix)
            => base.RunLoadAsync(load, failurePrefix);

        public new void UpdateContentState(bool hasVisibleItems)
            => base.UpdateContentState(hasVisibleItems);
    }

    [Fact]
    public void Constructor_StartsInLoadingState()
    {
        var vm = new TestPageViewModel();

        Assert.Equal(PageState.Loading, vm.State);
        Assert.True(vm.IsLoading);
        Assert.False(vm.IsContent);
        Assert.False(vm.HasError);
        Assert.Equal(string.Empty, vm.ErrorMessage);
    }

    [Fact]
    public async Task RunLoadAsync_Success_TransitionsToContent()
    {
        var vm = new TestPageViewModel();

        var result = await vm.RunLoadAsync(() => Task.CompletedTask, "加载失败");

        Assert.True(result);
        Assert.Equal(PageState.Content, vm.State);
        Assert.True(vm.IsContent);
    }

    [Fact]
    public async Task RunLoadAsync_Failure_GoesToErrorWithPrefixAndMessage()
    {
        var vm = new TestPageViewModel();

        var result = await vm.RunLoadAsync(
            () => throw new InvalidOperationException("数据源不可用"),
            "无法加载下载条目");

        Assert.False(result);
        Assert.Equal(PageState.Error, vm.State);
        Assert.True(vm.HasError);
        Assert.Contains("无法加载下载条目", vm.ErrorMessage);
        Assert.Contains("数据源不可用", vm.ErrorMessage);
    }

    [Fact]
    public async Task UpdateContentState_NoItems_GoesToEmpty()
    {
        var vm = new TestPageViewModel();
        await vm.RunLoadAsync(() => Task.CompletedTask, "加载失败");

        vm.UpdateContentState(hasVisibleItems: false);

        Assert.Equal(PageState.Empty, vm.State);
    }

    [Fact]
    public async Task UpdateContentState_WithItems_StaysContent()
    {
        var vm = new TestPageViewModel();
        await vm.RunLoadAsync(() => Task.CompletedTask, "加载失败");

        vm.UpdateContentState(hasVisibleItems: true);

        Assert.Equal(PageState.Content, vm.State);
    }

    [Fact]
    public void UpdateContentState_WhileLoading_DoesNotOverrideLoading()
    {
        var vm = new TestPageViewModel();

        vm.UpdateContentState(hasVisibleItems: false);

        Assert.Equal(PageState.Loading, vm.State);
    }

    [Fact]
    public async Task UpdateContentState_WhileErrored_DoesNotOverrideError()
    {
        var vm = new TestPageViewModel();
        await vm.RunLoadAsync(() => throw new Exception("boom"), "失败");

        vm.UpdateContentState(hasVisibleItems: true);

        Assert.Equal(PageState.Error, vm.State);
    }

    [Fact]
    public void DefaultEmptyStateTexts_HaveSensibleDefaults()
    {
        var vm = new TestPageViewModel();

        Assert.Equal("暂无内容", vm.EmptyTitle);
        Assert.Equal("\uE7B8", vm.EmptyGlyph);
    }
}
