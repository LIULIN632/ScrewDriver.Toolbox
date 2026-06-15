using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScrewDriver.Toolbox.Core;

namespace ScrewDriver.Toolbox.Core.Models;

public class CleanItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private long _sizeBytes;
    private int _fileCount;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconCode { get; set; } = "";
    public string IconColor { get; set; } = "#2563EB";
    public List<string> ScanPaths { get; set; } = new();
    public bool IsRecycler { get; set; }
    public bool IsRisk { get; set; }
    public string SpecialType { get; set; } = "";

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public long SizeBytes
    {
        get => _sizeBytes;
        set { _sizeBytes = value; OnPropertyChanged(); OnPropertyChanged(nameof(SizeText)); }
    }

    public int FileCount
    {
        get => _fileCount;
        set { _fileCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(CountText)); }
    }

    public string SizeText => SizeBytes == 0 ? "无文件" : FormatBytes(SizeBytes);
    public string CountText => FileCount == 0 ? "-" : $"{FileCount} 个文件";

    private static string FormatBytes(long bytes) => FileSizeFormatter.FormatBytes(bytes);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
