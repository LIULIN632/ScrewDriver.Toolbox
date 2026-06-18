using System.Collections.ObjectModel;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SystemOptimizerViewModel : BaseViewModel
{
    private string _pageTitle = "优化";
    private string _pageSubtitle = "优化您的 Windows 系统性能";
    private string _currentNav = "Optimize";

    public ObservableCollection<OptimizeCategoryItem> Categories { get; } = new();

    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    public string PageSubtitle
    {
        get => _pageSubtitle;
        set => SetProperty(ref _pageSubtitle, value);
    }

    public ICommand SwitchNavCommand { get; }

    public SystemOptimizerViewModel()
    {
        SwitchNavCommand = new RelayCommand(param =>
        {
            var nav = param as string ?? "Optimize";
            _currentNav = nav;
            if (nav == "Optimize")
            {
                PageTitle = "优化";
                PageSubtitle = "优化您的 Windows 系统性能";
                LoadOptimizeCategories();
            }
            else
            {
                PageTitle = "自定义";
                PageSubtitle = "按需开启或关闭系统功能";
                LoadCustomCategories();
            }
        });

        LoadOptimizeCategories();
    }

    private void LoadOptimizeCategories()
    {
        Categories.Clear();
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "隐私和安全", Description = "安全、内容交付和广告、锁屏、常规设置",
            IconCode = "🛡️", NewFeatureCount = 45
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "电源", Description = "显示、硬盘、Internet Explorer、桌面背景设置",
            IconCode = "🔋"
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "游戏和性能", Description = "处理器、显卡、网络、安全优化",
            IconCode = "🎮", NewFeatureCount = 17
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "更新", Description = "更新策略、交付和商店、更新行为",
            IconCode = "🔄", NewFeatureCount = 1
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "通知", Description = "其他设置、系统通知、隐私通知、安全通知",
            IconCode = "🔔"
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "声音", Description = "系统声音、音效方案设置",
            IconCode = "🔊"
        });
    }

    private void LoadCustomCategories()
    {
        Categories.Clear();
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "预装应用卸载", Description = "卸载 Windows 预装应用，释放系统空间",
            IconCode = "🗑️"
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "垃圾清理", Description = "扫描并清理系统临时文件和缓存",
            IconCode = "🧹"
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "系统信息", Description = "查看本机硬件配置和系统版本",
            IconCode = "ℹ️"
        });
    }
}
