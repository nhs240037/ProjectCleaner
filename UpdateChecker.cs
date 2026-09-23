using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace ProjectCleaner
{
  public class UpdateChecker
  {
    private const string Owner = "nhs240037";
    private const string Repo = "ProjectCleaner";

    // 実行中のアセンブリからバージョン（例: "1.0.0" や "1.1.1-dev.1"）を取得
    private static readonly string CurrentVersion = GetCurrentVersion();

    public enum VersionChannel
    {
      Stable,
      Beta,
      Alpha,
      Dev
    }

    /// <summary>
    /// 更新チェックと実行
    /// </summary>
    /// <param name="channel">VersionChannel{Stable, Beta, Alpha, Dev}</param>
    public static async Task CheckAndPerformUpdateAsync(VersionChannel channel)
    {
      try
      {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ProjectCleaner-Updater", "1.0"));

        JsonElement? targetRelease = null;

        if (channel == VersionChannel.Stable)
        {
          // Stableリリース取得
          string latestApiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
          HttpResponseMessage response = await client.GetAsync(latestApiUrl);
          if (response.IsSuccessStatusCode)
          {
            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);
            targetRelease = doc.RootElement.Clone();
          }
        }
        else
        {
          // プレビューチャンネル：リリース一覧から条件に合う最新リリースを検索
          string listApiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases?per_page=20";
          HttpResponseMessage response = await client.GetAsync(listApiUrl);
          if (response.IsSuccessStatusCode)
          {
            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            foreach (var rel in doc.RootElement.EnumerateArray())
            {
              string tagName = rel.GetProperty("tag_name").GetString() ?? "";

              //バージョン判定
              if (IsAllowedChannel(tagName, channel))
              {
                targetRelease = rel.Clone();
                break; // 最も新しい該当リリースを選択
              }
            }
          }
        }

        if (targetRelease == null) return;

        string latestVersion = targetRelease.Value.GetProperty("tag_name").GetString() ?? "";

        // 新しいバージョンが存在する場合
        if (IsNewerVersion(CurrentVersion, latestVersion))
        {
          bool isPrerelease = targetRelease.Value.GetProperty("prerelease").GetBoolean();
          string releaseTypeMsg = isPrerelease ? $"【プレビュー版 ({channel})】" : "【正式版】";

          var assets = targetRelease.Value.GetProperty("assets").EnumerateArray();
          var updateAsset = assets.FirstOrDefault(a =>
              a.GetProperty("name").GetString() == "ProjectCleaner-Update.zip");

          if (updateAsset.ValueKind != JsonValueKind.Undefined)
          {
            string downloadUrl = updateAsset.GetProperty("browser_download_url").GetString()!;

            var result = MessageBox.Show(
                $"新しい{releaseTypeMsg} ({latestVersion}) が利用可能です。\nアップデートして再起動しますか？",
                "アップデートの確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
              await DownloadAndSafeReplaceAsync(client, downloadUrl);
            }
          }
        }
      }
      catch (Exception ex)
      {
        // バックグラウンドでの自動チェック時はログ等にとどめ、エラーを握りつぶしてもOK
        Console.WriteLine($"更新チェックエラー: {ex.Message}");
      }
    }

    private static bool IsAllowedChannel(string tagName, VersionChannel userChannel)
    {
      bool isDev = tagName.Contains("-dev");
      bool isAlpha = tagName.Contains("-alpha");
      bool isBeta = tagName.Contains("-beta");

      // プレリリース表記がないものは正式版なのでどのチャンネルでも許可
      if (!isDev && !isAlpha && !isBeta) return true;

      return userChannel switch
      {
        VersionChannel.Dev => true,                       // dev は dev, alpha, beta すべて対象
        VersionChannel.Alpha => isAlpha || isBeta,        // alpha は alpha, beta のみ
        VersionChannel.Beta => isBeta,                    // beta は beta のみ
        _ => false
      };
    }

    private static string GetCurrentVersion()
    {
      var version = Assembly.GetExecutingAssembly()
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
          .InformationalVersion;

      if (!string.IsNullOrEmpty(version))
      {
        int plusIndex = version.IndexOf('+');
        if (plusIndex >= 0) version = version.Substring(0, plusIndex);
        return version;
      }
      return "0.0.0-dev";
    }

    private static bool IsNewerVersion(string currentStr, string latestStr)
    {
      // 正式部分（1.1.1）を取得
      string currentBase = currentStr.Split('-')[0];
      string latestBase = latestStr.Split('-')[0];

      if (Version.TryParse(currentBase, out var vCurrent) &&
          Version.TryParse(latestBase, out var vLatest))
      {
        if (vLatest > vCurrent) return true;
        if (vLatest < vCurrent) return false;

        // メジャー.マイナー.パッチが同一の場合 → プレリリース有無を評価
        bool currentIsPre = currentStr.Contains('-');
        bool latestIsPre = latestStr.Contains('-');

        // (例: 1.1.1-dev.1 から 1.1.1 への更新は許可)
        if (currentIsPre && !latestIsPre) return true;
        if (!currentIsPre && latestIsPre) return false;

        // 両方プレリリースの場合は文字列で新旧判定
        // (例: 1.1.1-dev.1 < 1.1.1-dev.2)
        if (currentIsPre && latestIsPre)
        {
          return string.CompareOrdinal(latestStr, currentStr) > 0;
        }
      }
      return false;
    }

    private static async Task DownloadAndSafeReplaceAsync(HttpClient client, string downloadUrl)
    {
      string tempDir = Path.Combine(Path.GetTempPath(), "ProjectCleanerUpdate");
      string zipPath = Path.Combine(tempDir, "ProjectCleaner-Update.zip");
      string appBaseDir = AppDomain.CurrentDomain.BaseDirectory;

      if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
      Directory.CreateDirectory(tempDir);

      // zip を一時ディレクトリにダウンロード＆展開
      byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
      await File.WriteAllBytesAsync(zipPath, fileBytes);
      ZipFile.ExtractToDirectory(zipPath, tempDir, overwriteFiles: true);
      File.Delete(zipPath); // ZIP自体は展開後削除

      // 展開されたファイルを現在のアプリディレクトリに退避上書き
      // (Windowsは仕様上、実行中のファイルであっても「名前の変更(.old)」は可能らしい...)
      foreach (string newFilePath in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
      {
        string relativePath = Path.GetRelativePath(tempDir, newFilePath);
        string targetPath = Path.Combine(appBaseDir, relativePath);

        // ディレクトリ構造の作成
        string? targetDir = Path.GetDirectoryName(targetPath);
        if (targetDir != null && !Directory.Exists(targetDir))
        {
          Directory.CreateDirectory(targetDir);
        }

        if (File.Exists(targetPath))
        {
          string oldFilePath = targetPath + ".old";
          if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
          File.Move(targetPath, oldFilePath);
        }

        // 新しいファイルを配置
        File.Copy(newFilePath, targetPath, overwrite: true);
      }

      // 一時ディレクトリの削除
      Directory.Delete(tempDir, true);

      // アプリを終了し、cmdを起動して .old ファイルの削除とアプリ再起動を行う
      RestartAndCleanOldFiles(appBaseDir);
    }

    private static void RestartAndCleanOldFiles(string appDir)
    {
      string exePath = Application.ExecutablePath;
      string cmdCommand = $"/C timeout /t 2 /nobreak > NUL & del /F /Q \"{appDir}\\*.old\" & start \"\" \"{exePath}\"";

      ProcessStartInfo psi = new ProcessStartInfo
      {
        FileName = "cmd.exe",
        Arguments = cmdCommand,
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true,
        UseShellExecute = false
      };

      Process.Start(psi);

      // 自プロセスを即座に終了する
      Application.Exit();
      Environment.Exit(0);
    }
  }
}