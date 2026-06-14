using System.Windows;
using System.Windows.Controls;
using ScrewDriver.Toolbox.Core.Models;
using ScrewDriver.Toolbox.UI.ViewModels;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;

namespace ScrewDriver.Toolbox.UI.Views;

public partial class SystemOptimizerPage : Page
{
    public SystemOptimizerPage()
    {
        InitializeComponent();
    }

    private void SettingCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { DataContext: SystemSettingItem setting }) return;
        if (DataContext is not SystemOptimizerViewModel vm) return;

        var enable = setting.IsEnabled;

        if (enable && setting.RiskLevel == RiskLevel.Dangerous)
        {
            setting.IsEnabled = false;

            var result = MessageBox.Show(
                $"「{setting.Name}」为高风险操作。\n\n{setting.RiskDescription}\n\n确定要执行吗？",
                "操作确认", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            setting.IsEnabled = true;
        }

        if (!vm.ApplySetting(setting.Id, enable))
        {
            setting.IsEnabled = !enable;
            MessageBox.Show($"「{setting.Name}」应用失败，请检查管理员权限。",
                "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RevertAll_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "确定要将所有设置恢复为默认值吗？\n\n此操作将撤销所有优化变更。",
            "确认恢复", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        if (DataContext is SystemOptimizerViewModel vm)
        {
            vm.RevertAll();
            vm.LoadSettings();
        }
    }

    private void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        new CleanWindow { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
