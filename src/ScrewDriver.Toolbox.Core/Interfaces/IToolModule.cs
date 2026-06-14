namespace ScrewDriver.Toolbox.Core.Interfaces;

public interface IToolModule
{
    string Name { get; }
    string Description { get; }
    string Category { get; }
    string? IconGlyph { get; }
}
