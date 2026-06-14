using ScrewDriver.Toolbox.Models;
using System.Collections.ObjectModel;

namespace ScrewDriver.Toolbox.Services;

/// <summary>
/// 工具目录服务
/// </summary>
public interface IToolCatalogService
{
    ObservableCollection<ToolModel> GetToolsByCategory(string category);
    ObservableCollection<string> GetCategories();
    ObservableCollection<ToolModel> SearchTools(string query);
    ToolModel? GetToolById(string id);
}
