namespace ScrewDriver.Toolbox.Core.Interfaces;

public interface IToolLauncher
{
    Task<int> LaunchAsync(string executablePath, string? arguments = null, string? workingDirectory = null);
    Task<int> LaunchElevatedAsync(string executablePath, string? arguments = null, string? workingDirectory = null);
    int Launch(string executablePath, string? arguments = null, string? workingDirectory = null);
    int LaunchElevated(string executablePath, string? arguments = null, string? workingDirectory = null);
}
