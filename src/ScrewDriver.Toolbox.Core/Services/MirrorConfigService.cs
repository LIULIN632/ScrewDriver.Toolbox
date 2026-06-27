using System.Diagnostics;
using System.Text.Json;

namespace ScrewDriver.Toolbox.Core.Services;

/// <summary>镜像源配置管理服务（参照 DevEnv Manager 的镜像换源设计）</summary>
public static class MirrorConfigService
{
    /// <summary>镜像源定义</summary>
    public record MirrorSource(string Name, string Url, string Description);

    /// <summary>npm 镜像源</summary>
    public static readonly List<MirrorSource> NpmMirrors = new()
    {
        new("官方源", "https://registry.npmjs.org/", "npm 官方源"),
        new("npmmirror", "https://registry.npmmirror.com/", "淘宝镜像源（推荐国内）"),
    };

    /// <summary>pip 镜像源</summary>
    public static readonly List<MirrorSource> PipMirrors = new()
    {
        new("官方源", "https://pypi.org/simple/", "PyPI 官方源"),
        new("清华源", "https://pypi.tuna.tsinghua.edu.cn/simple/", "清华大学镜像源（推荐国内）"),
        new("阿里源", "https://mirrors.aliyun.com/pypi/simple/", "阿里云镜像源"),
    };

    /// <summary>Go 镜像源</summary>
    public static readonly List<MirrorSource> GoMirrors = new()
    {
        new("官方源", "https://proxy.golang.org,direct", "Go 官方代理"),
        new("goproxy.cn", "https://goproxy.cn,direct", "七牛云镜像（推荐国内）"),
        new("goproxy.io", "https://goproxy.io,direct", "Go Proxy 中国镜像"),
    };

    /// <summary>获取当前 npm 源</summary>
    public static string GetCurrentNpmRegistry()
    {
        return GetCommandOutput("npm", "config get registry")?.Trim() ?? "未安装";
    }

    /// <summary>设置 npm 源</summary>
    public static bool SetNpmRegistry(string url)
    {
        return RunCommand("npm", $"config set registry {url}");
    }

    /// <summary>获取当前 pip 源</summary>
    public static string GetCurrentPipSource()
    {
        var output = GetCommandOutput("pip", "config get global.index-url");
        if (!string.IsNullOrWhiteSpace(output)) return output.Trim();
        return "官方源 (https://pypi.org/simple/)";
    }

    /// <summary>设置 pip 源</summary>
    public static bool SetPipSource(string url)
    {
        return RunCommand("pip", $"config set global.index-url {url}");
    }

    /// <summary>获取当前 Go 代理</summary>
    public static string GetCurrentGoProxy()
    {
        return GetCommandOutput("go", "env GOPROXY")?.Trim() ?? "未安装";
    }

    /// <summary>设置 Go 代理</summary>
    public static bool SetGoProxy(string url)
    {
        return RunCommand("go", $"env -w GOPROXY={url}");
    }

    private static string? GetCommandOutput(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return null;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return string.IsNullOrWhiteSpace(output) ? null : output.Trim();
        }
        catch { return null; }
    }

    private static bool RunCommand(string cmd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(cmd, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            p?.WaitForExit();
            return true;
        }
        catch { return false; }
    }
}
