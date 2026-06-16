namespace ScrewDriver.Toolbox.Core.Models;

public class WindowStateModel
{
    public double Left { get; set; } = double.NaN;
    public double Top { get; set; } = double.NaN;
    public double Width { get; set; } = 1100;
    public double Height { get; set; } = 700;
    public string State { get; set; } = "Normal";
}
