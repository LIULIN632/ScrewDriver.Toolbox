namespace ScrewDriver.Toolbox.Core.Services;

public class RecentToolsService
{
    private readonly JsonConfigManager _config;
    private List<RecentEntry> _entries;

    private const string ConfigName = "recent";

    public RecentToolsService()
    {
        _config = new JsonConfigManager(AppDomain.CurrentDomain.BaseDirectory);
        var data = _config.Load<RecentData>(ConfigName) ?? new RecentData();
        _entries = data.Entries ?? new List<RecentEntry>();
    }

    public void AddTool(string toolName, string pageTag)
    {
        _entries.RemoveAll(e => e.ToolName == toolName);
        _entries.Insert(0, new RecentEntry
        {
            ToolName = toolName,
            PageTag = pageTag,
            Timestamp = DateTime.Now
        });

        if (_entries.Count > 20)
            _entries = _entries.Take(20).ToList();

        _config.Save(ConfigName, new RecentData { Entries = _entries });
    }

    public List<(string ToolName, string PageTag, DateTime Time)> GetRecentTools(int count = 10)
        => _entries.Take(count).Select(e => (e.ToolName, e.PageTag, e.Timestamp)).ToList();

    private class RecentData
    {
        public List<RecentEntry> Entries { get; set; } = new();
    }

    private class RecentEntry
    {
        public string ToolName { get; set; } = string.Empty;
        public string PageTag { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
