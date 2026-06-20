using System.Text;
using Microsoft.Win32;

namespace ScrewDriver.Toolbox.Core.Services;

public static class SystemReportGenerator
{
    public static string GenerateTextReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("========================================");
        sb.AppendLine($" ScrewDriver 系统报告  生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("========================================");
        sb.AppendLine();

        sb.AppendLine("【1. 硬件概览】");
        try
        {
            sb.AppendLine($"设备型号：{SystemInfo.DetectHardwareBrand() ?? "未知"} {SystemInfo.DetectSystemModel() ?? ""}");
            sb.AppendLine($"CPU：{SystemInfo.GetProcessorName() ?? "未知"}");
            sb.AppendLine($"系统版本：{SystemInfo.WindowsVersionString}");
            sb.AppendLine($"管理员权限：{(SystemInfo.IsAdministrator ? "是" : "否")}");
        }
        catch { }

        sb.AppendLine();
        sb.AppendLine("【2. 系统信息】");
        sb.AppendLine($"计算机名：{Environment.MachineName}");
        sb.AppendLine($"运行时长：{TimeSpan.FromMilliseconds(Environment.TickCount).TotalHours:F1} 小时");
        sb.AppendLine($".NET 版本：{Environment.Version}");

        sb.AppendLine();
        sb.AppendLine("【3. 健康评分】");
        var score = CalculateHealthScore();
        sb.AppendLine($"综合健康评分：{score}/100");
        sb.AppendLine($"评级：{(score >= 80 ? "优秀" : score >= 60 ? "良好" : "需关注")}");

        sb.AppendLine();
        sb.AppendLine("【4. 优化建议】");
        if (!SystemInfo.IsAdministrator)
            sb.AppendLine("- 建议以管理员身份运行以获得完整优化能力");
        sb.AppendLine("========================================");
        sb.AppendLine(" 报告由 ScrewDriver Toolbox 生成");
        sb.AppendLine("========================================");

        Logger.Info("Text report generated");
        return sb.ToString();
    }

    public static string GenerateHtmlReport()
    {
        var score = CalculateHealthScore();
        var scoreColor = score >= 80 ? "#107c10" : score >= 60 ? "#f59e0b" : "#dc2626";

        return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>ScrewDriver 系统报告 {DateTime.Now:yyyy-MM-dd}</title>
<style>
body {{ font-family: 'Microsoft YaHei', sans-serif; max-width: 900px; margin: 20px auto; color: #333; background: #f5f7fa; }}
h1 {{ color: #2563eb; border-bottom: 2px solid #2563eb; padding-bottom: 8px; }}
h2 {{ color: #1e1e1e; margin-top: 24px; }}
.card {{ background: #fff; border-radius: 12px; padding: 20px; margin: 12px 0; box-shadow: 0 2px 8px rgba(0,0,0,0.08); }}
.score {{ font-size: 48px; font-weight: bold; color: {scoreColor}; text-align: center; }}
.item {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e5e7eb; }}
.label {{ color: #666; }}
.value {{ font-weight: 600; }}
.footer {{ text-align: center; color: #999; margin-top: 32px; font-size: 12px; }}
</style>
</head>
<body>
<h1>ScrewDriver 系统检测报告</h1>
<p>生成时间：{DateTime.Now:yyyy年MM月dd日 HH:mm:ss} | 计算机：{Environment.MachineName}</p>

<div class='card'>
<div class='score'>健康评分 {score}/100</div>
<p style='text-align:center;color:#666;'>{(score >= 80 ? "系统状态良好" : score >= 60 ? "建议进行系统优化" : "检测到潜在问题，请查看详情")}</p>
</div>

<h2>一、硬件概览</h2>
<div class='card'>
<div class='item'><span class='label'>设备品牌</span><span class='value'>{SystemInfo.DetectHardwareBrand() ?? "未知"}</span></div>
<div class='item'><span class='label'>设备型号</span><span class='value'>{SystemInfo.DetectSystemModel() ?? "未知"}</span></div>
<div class='item'><span class='label'>处理器</span><span class='value'>{SystemInfo.GetProcessorName() ?? "未知"}</span></div>
<div class='item'><span class='label'>系统版本</span><span class='value'>{SystemInfo.WindowsVersionString}</span></div>
<div class='item'><span class='label'>管理员权限</span><span class='value'>{(SystemInfo.IsAdministrator ? "是" : "否")}</span></div>
<div class='item'><span class='label'>运行时长</span><span class='value'>{TimeSpan.FromMilliseconds(Environment.TickCount).TotalHours:F1} 小时</span></div>
<div class='item'><span class='label'>.NET 版本</span><span class='value'>{Environment.Version}</span></div>
</div>

<div class='footer'>报告由 ScrewDriver Toolbox 生成 · {DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>
</body>
</html>";
    }

    public static int CalculateHealthScore()
    {
        int score = 100;
        if (!SystemInfo.IsAdministrator) score -= 10;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\wuauserv");
            if (key != null)
            {
                var start = (int)key.GetValue("Start", 2);
                if (start == 4) score -= 15; // Updates disabled
            }
        }
        catch { }
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");
            if (key?.GetValue("DisableRealtimeMonitoring") is int v && v == 1)
                score -= 20;
        }
        catch { }
        return Math.Max(0, score);
    }
}
