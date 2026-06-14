using CommunityToolkit.Mvvm.ComponentModel;

namespace ScrewDriver.Toolbox.ViewModels;

public partial class DataCenterViewModel : BaseViewModel
{
    public DataCenterViewModel()
    {
        PageTitle = "数据中心";
        PageDescription = "查看报告、备份与日志（开发中）";
    }
}
