namespace ScrewDriver.Toolbox.Core.Interfaces;

public interface IRiskOperation
{
    string RiskDescription { get; }
    bool CanRevert { get; }
    string RevertMethodDescription { get; }
}
