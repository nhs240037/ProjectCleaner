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
    Application.Run(new MainForm(args));
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


