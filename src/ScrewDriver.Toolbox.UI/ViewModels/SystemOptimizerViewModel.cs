using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.Views;
using WpfApp = System.Windows.Application;

namespace ScrewDriver.Toolbox.UI.ViewModels;

public class SystemOptimizerViewModel : BaseViewModel
{
    private string _pageTitle = "优化";
    private string _pageSubtitle = "优化您的 Windows 系统性能";

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
            if (nav == "Optimize")
            {
                PageTitle = "优化";
                PageSubtitle = "优化您的 Windows 系统性能";
                LoadOptimizeCategories();
            }
            else if (nav == "Custom")
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
            Title = "隐私和安全", Description = "安全, 内容交付和广告, 锁屏, 常规...",
            IconCode = "🛡️", NewFeatureCount = 45,
            NavigateCommand = new RelayCommand(_ => NavigateTo("隐私和安全"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "电源", Description = "显示, 硬盘, Internet Explorer, 桌面背景设置...",
            IconCode = "🔋",
            NavigateCommand = new RelayCommand(_ => NavigateTo("电源"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "游戏和性能", Description = "处理器, 显卡, 网络, 安全...",
            IconCode = "🎮", NewFeatureCount = 17,
            NavigateCommand = new RelayCommand(_ => NavigateTo("游戏和性能"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "更新", Description = "更新策略, 交付和商店, 更新行为",
            IconCode = "🔄", NewFeatureCount = 1,
            NavigateCommand = new RelayCommand(_ => NavigateTo("更新"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "通知", Description = "其他设置, 系统通知, 隐私通知, 安全通知",
            IconCode = "🔔",
            NavigateCommand = new RelayCommand(_ => NavigateTo("通知"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "声音", Description = "系统声音",
            IconCode = "🔊",
            NavigateCommand = new RelayCommand(_ => NavigateTo("声音"))
        });
    }

    private void LoadCustomCategories()
    {
        Categories.Clear();
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "预装应用卸载", Description = "卸载 Windows 预装应用",
            IconCode = "🗑️"
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "垃圾清理", Description = "扫描并清理系统临时文件和缓存",
            IconCode = "🧹"
        });
    }

    private static void NavigateTo(string category)
    {
        if (WpfApp.Current.MainWindow is MainWindow main)
            main.MainFrame.Navigate(new OptimizeDetailPage(category));
    }
}
