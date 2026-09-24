namespace ProjectCleaner;

internal enum CandidateKind
{
  File,
  Folder
}

internal sealed record Candidate(
    string Path,
    CandidateKind Kind,
    string SourceRoot,
    string Reason)
{
  public bool IsDirectory => Kind == CandidateKind.Folder;
}

internal static class Cleaner
{
  public static IReadOnlyList<Candidate> ScanRoots(IEnumerable<string> roots, Action<string>? log = null)
  {
    List<Candidate> results = [];
    HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
    HashSet<string> unityRoots = new(StringComparer.OrdinalIgnoreCase);

    foreach (string rootInput in roots)
    {
      if (string.IsNullOrWhiteSpace(rootInput))
      {
        continue;
      }

      string root = NormalizePath(rootInput);
      if (!Directory.Exists(root))
      {
        log?.Invoke($"存在しないためスキップ: {root}");
        continue;
      }

      log?.Invoke($"検査中: {root}");

      ScanVisualStudio(root, results, seen);
      ScanUnity(root, results, seen, log, unityRoots);
      ScanDotNet(root, results, seen);
    }

    return results;
  }

  public static bool TryDelete(Candidate candidate, out string error)
  {
    error = string.Empty;

    try
    {
      if (candidate.IsDirectory)
      {
        if (Directory.Exists(candidate.Path))
        {
          Directory.Delete(candidate.Path, recursive: true);
        }
      }
      else
      {
        if (File.Exists(candidate.Path))
        {
          File.Delete(candidate.Path);
        }
      }

      return candidate.IsDirectory ? !Directory.Exists(candidate.Path) : !File.Exists(candidate.Path);
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
  }

  private static void ScanVisualStudio(string root, List<Candidate> results, HashSet<string> seen)
  {
    string vsRoot = Path.Combine(root, ".vs");
    if (!Directory.Exists(vsRoot))
    {
      return;
    }

    foreach (string ipchFolder in SafeEnumerateDirectories(vsRoot, "ipch"))
    {
      AddFolderIfExists(ipchFolder, root, "Visual Studio IntelliSenseキャッシュ", results, seen);
    }

    AddFileIfExists(Path.Combine(vsRoot, "Browse.VC.db"), root, "Visual Studio browse DB", results, seen);
    AddFileIfExists(Path.Combine(vsRoot, "Insiders", "Browse.VC.db"), root, "Visual Studio browse DB", results, seen);

    foreach (string file in SafeEnumerateFiles(vsRoot, "*.suo"))
    {
      AddFileIfExists(file, root, "Visual Studio ユーザーオプション", results, seen);
    }

    foreach (string file in SafeEnumerateFiles(vsRoot, "*.VC.db"))
    {
      AddFileIfExists(file, root, "Visual Studio データベース", results, seen);
    }

    foreach (string file in SafeEnumerateFiles(vsRoot, "*.opendb"))
    {
      AddFileIfExists(file, root, "Visual Studio オープンデータベース", results, seen);
    }

    //foreach (string file in SafeEnumerateFiles(vsRoot, "*Pikachu_Hoodie*.fbx"))
    //{
    //  AddFileIfExists(file, root, "一般男性のハイポリモデル", results, seen);
    //}
    //foreach (string file in SafeEnumerateFiles(vsRoot, "*Pikachu_Hoodie*.png"))
    //{
    //  AddFileIfExists(file, root, "一般男性の画像", results, seen);
    //}
  }

  private static void ScanUnity(string root, List<Candidate> results, HashSet<string> seen, Action<string>? log, HashSet<string> unityRoots)
  {
    foreach (string unityRoot in EnumerateUnityRoots(root, unityRoots))
    {
      log?.Invoke($"| Unityモードで検査中: {unityRoot}");

      AddFolderIfExists(Path.Combine(unityRoot, "Library"), unityRoot, "Unity ライブラリキャッシュ", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "Temp"), unityRoot, "Unity キャッシュ", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "Obj"), unityRoot, "Unity オブジェクトキャッシュ", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "Logs"), unityRoot, "Unity ログファイル", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "MemoryCaptures"), unityRoot, "Unity キャプチャ履歴", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "Build"), unityRoot, "Unity ビルド", results, seen);
      AddFolderIfExists(Path.Combine(unityRoot, "Builds"), unityRoot, "Unity ビルド", results, seen);
      //foreach (string file in SafeEnumerateFiles(unityRoot, "*Pikachu_Hoodie*.fbx"))
      //{
      //  AddFileIfExists(file, root, "一般男性のハイポリモデル", results, seen);
      //}
      //foreach (string file in SafeEnumerateFiles(unityRoot, "*Pikachu_Hoodie*.png"))
      //{
      //  AddFileIfExists(file, root, "一般男性の画像", results, seen);
      //}
    }
  }

  private static IEnumerable<string> EnumerateUnityRoots(string root, HashSet<string> unityRoots)
  {
    string directProjectVersion = Path.Combine(root, "ProjectSettings", "ProjectVersion.txt");
    if (File.Exists(directProjectVersion))
    {
      string normalized = NormalizePath(root);
      if (unityRoots.Add(normalized))
      {
        yield return normalized;
      }
    }

    foreach (string projectVersion in SafeEnumerateFiles(root, "ProjectVersion.txt", SearchOption.AllDirectories))
    {
      string? projectSettings = Path.GetDirectoryName(projectVersion);
      if (string.IsNullOrWhiteSpace(projectSettings))
      {
        continue;
      }

      string? unityRoot = Path.GetDirectoryName(projectSettings);
      if (string.IsNullOrWhiteSpace(unityRoot))
      {
        continue;
      }

      string normalized = NormalizePath(unityRoot);
      if (unityRoots.Add(normalized))
      {
        yield return normalized;
      }
    }
  }

  private static void ScanDotNet(string root, List<Candidate> results, HashSet<string> seen)
  {
    if (!SafeEnumerateFiles(root, "*.sln", SearchOption.TopDirectoryOnly).Any())
    {
      return;
    }

    AddFolderIfExists(Path.Combine(root, "bin"), root, ".Net ビルド", results, seen);
    AddFolderIfExists(Path.Combine(root, "obj"), root, ".Net オブジェクトキャッシュ", results, seen);
  }

  private static void AddFileIfExists(string path, string root, string reason, List<Candidate> results, HashSet<string> seen)
  {
    string normalized = NormalizePath(path);
    if (!File.Exists(normalized))
    {
      return;
    }

    if (seen.Add(normalized))
    {
      results.Add(new Candidate(normalized, CandidateKind.File, root, reason));
    }
  }

  private static void AddFolderIfExists(string path, string root, string reason, List<Candidate> results, HashSet<string> seen)
  {
    string normalized = NormalizePath(path);
    if (!Directory.Exists(normalized))
    {
      return;
    }

    if (seen.Add(normalized))
    {
      results.Add(new Candidate(normalized, CandidateKind.Folder, root, reason));
    }
  }

  private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern, SearchOption option = SearchOption.AllDirectories)
  {
    try
    {
      return Directory.EnumerateFiles(root, pattern, option);
    }
    catch
    {
      return Array.Empty<string>();
    }
  }

  private static IEnumerable<string> SafeEnumerateDirectories(string root, string pattern, SearchOption option = SearchOption.AllDirectories)
  {
    try
    {
      return Directory.EnumerateDirectories(root, pattern, option);
    }
    catch
    {
      return Array.Empty<string>();
    }
  }

  private static string NormalizePath(string path)
  {
    return Path.GetFullPath(path);
  }
}


