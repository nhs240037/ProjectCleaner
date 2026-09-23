using System.Reflection;

namespace ProjectCleaner;

internal static class Program
{
  [STAThread]
  private static void Main(string[] args)
  {
    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // 起動引数の解析
    UpdateChecker.VersionChannel channel = UpdateChecker.VersionChannel.Stable;

    string? channelArg = args.FirstOrDefault(a => a.StartsWith("--version-channel=", StringComparison.OrdinalIgnoreCase));
    if (channelArg != null)
    {
      string channelValue = channelArg.Split('=')[1].ToLowerInvariant();
      channel = channelValue switch
      {
        "dev" => UpdateChecker.VersionChannel.Dev,
        "alpha" => UpdateChecker.VersionChannel.Alpha,
        "beta" => UpdateChecker.VersionChannel.Beta,
        _ => UpdateChecker.VersionChannel.Stable
      };
    }
    Application.Run(new MainForm(channel));
  }

  private static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
  {
    string assemblyName = new AssemblyName(args.Name).Name + ".dll";
    string libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", assemblyName);

    if (File.Exists(libPath))
      return Assembly.LoadFrom(libPath);

    return null;
  }
}


