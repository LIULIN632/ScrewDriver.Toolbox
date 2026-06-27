using Microsoft.Win32;
using System.Diagnostics;

namespace ScrewDriver.Toolbox.Core.Services;

/// <summary>Windows 环境变量诊断服务（参照 DevEnv Manager 的设计）</summary>
public static class EnvironmentDiagnosticService
{
    /// <summary>诊断项</summary>
    public record DiagnosticItem(string Name, string Status, string Message);

    /// <summary>版本信息</summary>
    public record RuntimeInfo(string Name, string Version, string Path, bool IsInstalled);

    /// <summary>运行环境诊断</summary>
    public static List<DiagnosticItem> RunDiagnostics()
    {
        var items = new List<DiagnosticItem>();

        // 1. 检查关键环境变量
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User)
                    ?? Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine);
        var pythonPath = Environment.GetEnvironmentVariable("PYTHON_HOME", EnvironmentVariableTarget.User)
                      ?? Environment.GetEnvironmentVariable("PYTHON_HOME", EnvironmentVariableTarget.Machine);
        var nodePath = Environment.GetEnvironmentVariable("NODE_HOME", EnvironmentVariableTarget.User)
                    ?? Environment.GetEnvironmentVariable("NODE_HOME", EnvironmentVariableTarget.Machine);

        items.Add(Check("JAVA_HOME", !string.IsNullOrEmpty(javaHome), javaHome ?? "未配置"));
        items.Add(Check("PYTHON_HOME", !string.IsNullOrEmpty(pythonPath), pythonPath ?? "未配置"));
        items.Add(Check("NODE_HOME", !string.IsNullOrEmpty(nodePath), nodePath ?? "未配置"));

        // 2. 检查 PATH 中关键路径
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
        var machinePath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? "";
        var fullPath = $"{userPath};{machinePath}".ToLower();

        var keyPaths = new[] { "java", "python", "node", "npm", "git", "docker" };
        foreach (var keyword in keyPaths)
        {
            var found = fullPath.Split(';').Any(p => p.ToLower().Contains(keyword));
            items.Add(Check($"PATH: {keyword}", found, found ? "已配置" : "缺失"));
        }

        // 3. 检查当前运行时版本
        items.Add(CheckRuntimeVersion("Java", "java", "-version"));
        items.Add(CheckRuntimeVersion("Python", "python", "--version"));
        items.Add(CheckRuntimeVersion("Node.js", "node", "--version"));
        items.Add(CheckRuntimeVersion("Git", "git", "--version"));
        items.Add(CheckRuntimeVersion("Docker", "docker", "--version"));

        return items;
    }

    /// <summary>获取多版本运行时信息</summary>
    public static List<RuntimeInfo> DiscoverRuntimes()
    {
        var runtimes = new List<RuntimeInfo>();

        // Java
        DiscoverJava(runtimes);
        // Python
        DiscoverPython(runtimes);
        // Node.js
        DiscoverNode(runtimes);

        return runtimes;
    }

    private static void DiscoverJava(List<RuntimeInfo> runtimes)
    {
        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User)
                        ?? Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaPath = Path.Combine(javaHome, "bin", "java.exe");
                var version = GetCommandOutput("java", "-version");
                runtimes.Add(new RuntimeInfo("Java", version, javaHome, File.Exists(javaPath)));
            }
            else
            {
                var version = GetCommandOutput("java", "-version");
                runtimes.Add(new RuntimeInfo("Java", version ?? "未安装", "", !string.IsNullOrEmpty(version)));
            }
        }
        catch { }
    }

    private static void DiscoverPython(List<RuntimeInfo> runtimes)
    {
        try
        {
            var version = GetCommandOutput("python", "--version");
            var path = GetCommandOutput("python", "-c \"import sys; print(sys.executable)\"");
            runtimes.Add(new RuntimeInfo("Python", version ?? "未安装", path ?? "", !string.IsNullOrEmpty(version)));
        }
        catch { }
    }

    private static void DiscoverNode(List<RuntimeInfo> runtimes)
    {
        try
        {
            var version = GetCommandOutput("node", "--version");
            var path = GetCommandOutput("node", "-e \"console.log(process.execPath)\"");
            runtimes.Add(new RuntimeInfo("Node.js", version ?? "未安装", path ?? "", !string.IsNullOrEmpty(version)));
        }
        catch { }
    }

    private static DiagnosticItem Check(string name, bool ok, string message)
        => new(name, ok ? "✅ 正常" : "⚠️ 需关注", message);

    private static DiagnosticItem CheckRuntimeVersion(string name, string cmd, string args)
    {
        var version = GetCommandOutput(cmd, args);
        return new DiagnosticItem($"当前 {name}",
            string.IsNullOrEmpty(version) ? "❌ 未安装" : "✅ 正常",
            version ?? "未安装");
    }

    private static string? GetCommandOutput(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return null;
            var output = p.StandardOutput.ReadToEnd();
            var err = p.StandardError.ReadToEnd();
            p.WaitForExit();
            var result = !string.IsNullOrWhiteSpace(output) ? output.Trim() : err.Trim();
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        catch { return null; }
    }
}
