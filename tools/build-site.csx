#!/usr/bin/env dotnet-script

using System.Diagnostics;

var repoRoot = Args.Count > 0 && !string.IsNullOrWhiteSpace(Args[0])
    ? Path.GetFullPath(Args[0])
    : Directory.GetCurrentDirectory();

RunDotnetScript(Path.Combine(repoRoot, "tools", "build-writing.csx"), repoRoot);
RunDotnetScript(Path.Combine(repoRoot, "tools", "build-projects.csx"), repoRoot);

Console.WriteLine("Site build complete: writing + projects.");

static void RunDotnetScript(string scriptPath, string repoRoot)
{
    if (!File.Exists(scriptPath))
        throw new FileNotFoundException("Script not found", scriptPath);

    var psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        WorkingDirectory = repoRoot,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    psi.ArgumentList.Add("script");
    psi.ArgumentList.Add(scriptPath);
    psi.ArgumentList.Add(repoRoot);

    using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet script.");
    process.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.WriteLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Console.Error.WriteLine(e.Data); };
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    if (process.ExitCode != 0)
        throw new Exception($"Script failed with code {process.ExitCode}: {scriptPath}");
}
