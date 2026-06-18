using System.Windows.Input;

namespace ScrewDriver.Toolbox.Core.Models;

public class OptimizeCategoryItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public int NewFeatureCount { get; set; }
    public bool HasNewBadge => NewFeatureCount > 0;
    public string NewBadgeText => NewFeatureCount > 0 ? $"新功能 {NewFeatureCount}" : "";
    public ICommand? NavigateCommand { get; set; }
}
