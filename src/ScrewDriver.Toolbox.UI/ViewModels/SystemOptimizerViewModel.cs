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
            Title = "隐私与体验", Description = "广告、诊断数据、体验优化、云剪贴板",
            IconCode = "🛡️",
            NavigateCommand = new RelayCommand(_ => NavigateTo("隐私与体验"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "性能优化", Description = "GPU调度、快速启动、动画效果、核心隔离",
            IconCode = "⚡",
            NavigateCommand = new RelayCommand(_ => NavigateTo("性能优化"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "游戏优化", Description = "游戏模式、独显直连、窗口优化、鼠标加速",
            IconCode = "🎮",
            NavigateCommand = new RelayCommand(_ => NavigateTo("游戏优化"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "电源与电池", Description = "电源计划、休眠、养护模式、节能",
            IconCode = "🔋",
            NavigateCommand = new RelayCommand(_ => NavigateTo("电源与电池"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "存储与后台", Description = "存储感知、传递优化、后台服务、启动延迟",
            IconCode = "💾",
            NavigateCommand = new RelayCommand(_ => NavigateTo("存储与后台"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "搜索与助手", Description = "搜索亮点、Copilot、小娜、历史记录",
            IconCode = "🔍",
            NavigateCommand = new RelayCommand(_ => NavigateTo("搜索与助手"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "更新与传输", Description = "更新策略、传递优化、驱动更新、商店更新",
            IconCode = "🔄",
            NavigateCommand = new RelayCommand(_ => NavigateTo("更新与传输"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "安全", Description = "UAC、BitLocker、Defender、SmartScreen、VBS",
            IconCode = "🔒",
            NavigateCommand = new RelayCommand(_ => NavigateTo("安全"))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "通知与个性化", Description = "通知、深色模式、扩展名、隐藏文件、开机声",
            IconCode = "🔔",
            NavigateCommand = new RelayCommand(_ => NavigateTo("通知与个性化"))
        });
    }

    private void LoadCustomCategories()
    {
        Categories.Clear();
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "预装应用卸载", Description = "卸载 Windows 预装应用",
            IconCode = "🗑️",
            NavigateCommand = new RelayCommand(_ => OpenWindow(new CleanWindow()))
        });
        Categories.Add(new OptimizeCategoryItem
        {
            Title = "垃圾清理", Description = "扫描并清理系统临时文件和缓存",
            IconCode = "🧹",
            NavigateCommand = new RelayCommand(_ => OpenWindow(new CleanWindow()))
        });
    }

    private static void OpenWindow(Window w)
    {
        if (WpfApp.Current.MainWindow is Window main)
        {
            w.Owner = main;
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.ShowDialog();
        }
    }

    private static void NavigateTo(string category)
    {
        if (WpfApp.Current.MainWindow is MainWindow main)
            main.MainFrame.Navigate(new OptimizeDetailPage(category));
    }
}
