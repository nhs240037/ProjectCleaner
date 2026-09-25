using System.Reflection;
using System.Text.Json;

namespace ProjectCleaner;

internal sealed class MainForm : Form
{
  private readonly ListBox _rootsList = new();
  private readonly ListView _targetsView = new();
  private readonly TextBox _logBox = new();
  private readonly Button _scanButton = new();
  private readonly Button _deleteButton = new();
  private readonly Button _addButton = new();
  private readonly Button _removeButton = new();
  private readonly Button _selectAllButton = new();
  private readonly Button _selectNoneButton = new();
  private readonly Label _summaryLabel = new();
  private readonly PseudoProgressBar _pseudoProgressBar = new();
  private readonly Panel _summaryProgressPanel = new();
  private readonly string _stateFilePath;
  private bool _suspendAutoSave;

  // アップデートチェック用のチャンネル（デフォルトは Stable）
  private readonly UpdateChecker.VersionChannel _versionChannel = UpdateChecker.VersionChannel.Stable;

  // 1. 既存のコンストラクタ（initialRoots を受け取る）
  public MainForm(IEnumerable<string>? initialRoots = null)
  {
    string currentVersion = GetCurrentVersion();

    Text = $"Project Cleaner [{currentVersion}]";
    StartPosition = FormStartPosition.CenterScreen;
    MinimumSize = new Size(1100, 720);
    Font = SystemFonts.MessageBoxFont;
    _stateFilePath = GetStateFilePath();

    BuildLayout();
    WireEvents();

    _suspendAutoSave = true;
    try
    {
      LoadSavedRoots();

      foreach (string root in initialRoots ?? Array.Empty<string>())
      {
        AddRoot(root);
      }
    }
    finally
    {
      _suspendAutoSave = false;
    }

    SaveRoots();

    UpdateSummary();

    // フォームが表示完了したタイミングでアップデートチェックを実行
    Shown += MainForm_Shown;
  }

  private static string GetCurrentVersion()
  {
    string? version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;

    if (!string.IsNullOrEmpty(version))
    {
      // '+' (ビルドメタデータ/コミットハッシュ) が含まれていれば削除
      int plusIndex = version.IndexOf('+');
      if (plusIndex >= 0) version = version[..plusIndex];
      return version;
    }

    return "0.0.0-dev"; // 取得できない場合のデフォルト値
  }

  public MainForm(UpdateChecker.VersionChannel channel, IEnumerable<string>? initialRoots = null)
      : this(initialRoots)
  {
    _versionChannel = channel;
  }

  private async void MainForm_Shown(object? sender, EventArgs e)
  {
    // UI表示をブロックせずに非同期でアップデートチェック
    await UpdateChecker.CheckAndPerformUpdateAsync(_versionChannel);
  }

  private void BuildLayout()
  {
    Panel rootPanel = new()
    {
      Dock = DockStyle.Left,
      Width = 320,
      Padding = new Padding(12)
    };

    Panel targetPanel = new()
    {
      Dock = DockStyle.Fill,
      Padding = new Padding(12)
    };

    Controls.Add(targetPanel);
    Controls.Add(rootPanel);

    Label rootTitle = new()
    {
      Text = "検査対象プロジェクト",
      Dock = DockStyle.Top,
      Height = 24,
      Font = new Font(Font, FontStyle.Bold)
    };

    _rootsList.Dock = DockStyle.Fill;
    _rootsList.SelectionMode = SelectionMode.MultiExtended;

    FlowLayoutPanel rootButtonBar = new()
    {
      Dock = DockStyle.Bottom,
      Height = 40,
      FlowDirection = FlowDirection.LeftToRight,
      WrapContents = false
    };

    _addButton.Text = "追加";
    _removeButton.Text = "削除";
    _addButton.Width = 80;
    _removeButton.Width = 80;

    rootButtonBar.Controls.AddRange(new Control[] { _addButton, _removeButton });

    rootPanel.Controls.Add(_rootsList);
    rootPanel.Controls.Add(rootButtonBar);
    rootPanel.Controls.Add(rootTitle);

    Label targetTitle = new()
    {
      Text = "クリーンアップ対象",
      Dock = DockStyle.Top,
      Height = 24,
      Font = new Font(Font, FontStyle.Bold)
    };

    _targetsView.Dock = DockStyle.Fill;
    _targetsView.CheckBoxes = true;
    _targetsView.FullRowSelect = true;
    _targetsView.GridLines = true;
    _targetsView.HideSelection = false;
    _targetsView.View = View.Details;
    _ = _targetsView.Columns.Add("タイプ", 130);
    _ = _targetsView.Columns.Add("ファイル/フォルダパス", 420);
    _ = _targetsView.Columns.Add("種類", 180);

    FlowLayoutPanel targetButtonBar = new()
    {
      Dock = DockStyle.Bottom,
      Height = 44,
      FlowDirection = FlowDirection.LeftToRight,
      WrapContents = false
    };

    _scanButton.Text = "スキャン";
    _deleteButton.Text = "掃除する";
    _selectAllButton.Text = "全選択";
    _selectNoneButton.Text = "選択解除";

    foreach (Button? button in new[] { _scanButton, _deleteButton, _selectAllButton, _selectNoneButton })
    {
      button.Width = 110;
    }
    Padding defaultMargin = _selectAllButton.Margin;
    defaultMargin.Left = 280;
    _scanButton.Margin = defaultMargin;
    _deleteButton.ForeColor = Color.Red;

    targetButtonBar.Controls.AddRange(new Control[] { _selectAllButton, _selectNoneButton, _scanButton, _deleteButton });

    _logBox.Dock = DockStyle.Bottom;
    _logBox.Multiline = true;
    _logBox.ReadOnly = true;
    _logBox.ScrollBars = ScrollBars.Vertical;
    _logBox.Height = 180;
    _logBox.BackColor = Color.White;

    _summaryProgressPanel.Dock = DockStyle.Bottom;
    _summaryProgressPanel.Height = 28;
    _summaryProgressPanel.Padding = new Padding(0, 0, 0, 0);
    _summaryProgressPanel.BorderStyle = BorderStyle.None;

    _summaryLabel.Dock = DockStyle.Left;
    _summaryLabel.Width = 300;
    _summaryLabel.AutoSize = false;
    _summaryLabel.TextAlign = ContentAlignment.MiddleLeft;
    _summaryLabel.Padding = new Padding(4, 6, 0, 4);

    _pseudoProgressBar.Dock = DockStyle.Fill;

    _summaryProgressPanel.Controls.Add(_summaryLabel);
    _summaryProgressPanel.Controls.Add(_pseudoProgressBar);

    targetPanel.Controls.Add(_targetsView);
    targetPanel.Controls.Add(targetButtonBar);
    targetPanel.Controls.Add(_summaryProgressPanel);
    targetPanel.Controls.Add(_logBox);
    targetPanel.Controls.Add(targetTitle);

    _logBox.Height = 180;
    _logBox.BackColor = Color.White;
  }

  private void WireEvents()
  {
    _addButton.Click += (_, _) => AddRootFromDialog();
    _removeButton.Click += (_, _) => RemoveSelectedRoots();
    _scanButton.Click += async (_, _) => await ScanAsync();
    _deleteButton.Click += async (_, _) => await DeleteCheckedAsync();
    _selectAllButton.Click += (_, _) => SetAllChecked(true);
    _selectNoneButton.Click += (_, _) => SetAllChecked(false);
    _rootsList.DoubleClick += (_, _) => AddRootFromDialog();
    _targetsView.ItemChecked += (_, _) => UpdateSummary();
    FormClosing += (_, _) => SaveRoots();
    AllowDrop = true;
    DragEnter += OnDragEnter;
    DragDrop += OnDragDrop;
  }

  private void AddRootFromDialog()
  {
    string[] selected = FolderPicker.PickFolders(this, "プロジェクトのフォルダを追加");
    if (selected.Length == 0)
    {
      return;
    }

    foreach (string path in selected)
    {
      AddRoot(path);
    }
  }

  private void AddRoot(string root)
  {
    if (string.IsNullOrWhiteSpace(root))
    {
      return;
    }

    string normalized = Path.GetFullPath(root);
    for (int i = 0; i < _rootsList.Items.Count; i++)
    {
      if (string.Equals(_rootsList.Items[i]?.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }
    }

    _ = _rootsList.Items.Add(normalized);
    Log($"プロジェクト追加: {normalized}");
    if (!_suspendAutoSave)
    {
      SaveRoots();
    }
  }

  private void RemoveSelectedRoots()
  {
    List<object> selected = _rootsList.SelectedItems.Cast<object>().ToList();
    foreach (object? item in selected)
    {
      _rootsList.Items.Remove(item);
    }

    if (!_suspendAutoSave)
    {
      SaveRoots();
    }
    UpdateSummary();
  }

  private async System.Threading.Tasks.Task ScanAsync()
  {
    string[] roots = _rootsList.Items.Cast<object>().Select(item => item.ToString() ?? string.Empty).ToArray();
    if (roots.Length == 0)
    {
      _ = MessageBox.Show(this, "プロジェクトを1つは追加してください", "Project Cleaner", MessageBoxButtons.OK, MessageBoxIcon.Information);
      return;
    }

    SetBusy(true);
    _pseudoProgressBar.SetVisible(true);
    _pseudoProgressBar.Start(roots.Length, roots.Length);
    _summaryLabel.Text = "検査中: 0/" + roots.Length;
    Log("スキャン開始.");

    IReadOnlyList<Candidate> candidates = Array.Empty<Candidate>();
    try
    {
      candidates = await System.Threading.Tasks.Task.Run(() =>
        Cleaner.ScanRoots(roots, Log, (current, total) => UpdateScanProgress(current, total)));
    }
    finally
    {
      SetBusy(false);
      _pseudoProgressBar.HideProgress();
    }

    PopulateTargets(candidates);
    Log($"スキャン完了. {candidates.Count} 個のゴミが発掘されました.");
    UpdateSummary();
  }

  private void UpdateScanProgress(int current, int total)
  {
    if (InvokeRequired)
    {
      _ = BeginInvoke(new Action<int, int>(UpdateScanProgress), current, total);
      return;
    }
    _pseudoProgressBar.Update(current, total);
    _summaryLabel.Text = total <= 0 ? "検査中: n/a" : $"検査中: {current}/{total}";
  }

  private void PopulateTargets(IReadOnlyList<Candidate> candidates)
  {
    _targetsView.BeginUpdate();
    try
    {
      _targetsView.Items.Clear();
      foreach (Candidate candidate in candidates)
      {
        string type = candidate.IsDirectory ? "フォルダ" : "ファイル";
        ListViewItem item = new(type)
        {
          Checked = true,
          Tag = candidate
        };
        _ = item.SubItems.Add(candidate.Path);
        _ = item.SubItems.Add(candidate.Reason);
        _ = _targetsView.Items.Add(item);
      }
    }
    finally
    {
      _targetsView.EndUpdate();
    }
  }

  private async System.Threading.Tasks.Task DeleteCheckedAsync()
  {
    List<Candidate> checkedCandidates = _targetsView.CheckedItems
        .Cast<ListViewItem>()
        .Select(item => item.Tag as Candidate)
        .Where(candidate => candidate is not null)
        .Cast<Candidate>()
        .ToList();

    if (checkedCandidates.Count == 0)
    {
      _ = MessageBox.Show(this, "掃除する対象が選択されていません", "Project Cleaner", MessageBoxButtons.OK, MessageBoxIcon.Information);
      return;
    }

    DialogResult result = MessageBox.Show(
        this,
        $"{checkedCandidates.Count} 個の選択されたゴミを掃除しますか？\n" +
        $"対象のプロジェクトは次回の起動時、時間がかかります",
        "Project Cleaner",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning);

    if (result != DialogResult.Yes)
    {
      return;
    }

    SetBusy(true);
    try
    {
      await System.Threading.Tasks.Task.Run(() =>
      {
        foreach (Candidate candidate in checkedCandidates)
        {
          Log($"{(candidate.IsDirectory ? "フォルダ" : "ファイル")}: {candidate.Path}");
          if (Cleaner.TryDelete(candidate, out string? error))
          {
            Log("→ 削除済み");
          }
          else
          {
            Log($"→ 削除失敗: {error}");
          }
        }
      });
    }
    finally
    {
      SetBusy(false);
    }

    List<ListViewItem> remaining = _targetsView.Items.Cast<ListViewItem>().Where(item => item.Checked).ToList();
    foreach (ListViewItem item in remaining)
    {
      if (item.Tag is not Candidate candidate)
      {
        continue;
      }

      bool exists = candidate.IsDirectory ? Directory.Exists(candidate.Path) : File.Exists(candidate.Path);
      if (!exists)
      {
        _targetsView.Items.Remove(item);
      }
    }

    Log("掃除が完了しました.");
    UpdateSummary();
  }

  private void SetAllChecked(bool isChecked)
  {
    _targetsView.BeginUpdate();
    try
    {
      foreach (ListViewItem item in _targetsView.Items)
      {
        item.Checked = isChecked;
      }
    }
    finally
    {
      _targetsView.EndUpdate();
    }

    UpdateSummary();
  }

  private void UpdateSummary()
  {
    int total = _targetsView.Items.Count;
    int checkedCount = _targetsView.CheckedItems.Count;
    _summaryLabel.Text = $"Roots: {_rootsList.Items.Count}    Targets: {total}    Checked: {checkedCount}";
  }

  private void SetBusy(bool busy)
  {
    _scanButton.Enabled = !busy;
    _deleteButton.Enabled = !busy;
    _addButton.Enabled = !busy;
    _removeButton.Enabled = !busy;
    _selectAllButton.Enabled = !busy;
    _selectNoneButton.Enabled = !busy;
    UseWaitCursor = busy;
  }

  private void Log(string message)
  {
    if (InvokeRequired)
    {
      _ = BeginInvoke(new Action(() => Log(message)));
      return;
    }

    string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
    _logBox.AppendText(line + Environment.NewLine);
  }

  private void OnDragEnter(object? sender, DragEventArgs e)
  {
    if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
    {
      e.Effect = DragDropEffects.Copy;
    }
  }

  private void OnDragDrop(object? sender, DragEventArgs e)
  {
    if (e.Data?.GetData(DataFormats.FileDrop) is not string[] dropped)
    {
      return;
    }

    foreach (string path in dropped)
    {
      if (Directory.Exists(path))
      {
        AddRoot(path);
      }
    }
  }

  private void LoadSavedRoots()
  {
    try
    {
      if (!File.Exists(_stateFilePath))
      {
        return;
      }

      string json = File.ReadAllText(_stateFilePath);
      AppState? state = JsonSerializer.Deserialize<AppState>(json);
      if (state?.Roots is null)
      {
        return;
      }

      foreach (string root in state.Roots)
      {
        AddRoot(root);
      }

      Log($"{state.Roots.Length} 個の検査対象を復元しました.");
    }
    catch (Exception ex)
    {
      Log($"前回の設定の復元に失敗しました: {ex.Message}");
    }
  }

  private void SaveRoots()
  {
    try
    {
      string[] roots = _rootsList.Items.Cast<object>()
          .Select(item => item.ToString() ?? string.Empty)
          .Where(path => !string.IsNullOrWhiteSpace(path))
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .ToArray();

      string? directory = Path.GetDirectoryName(_stateFilePath);
      if (!string.IsNullOrWhiteSpace(directory))
      {
        _ = Directory.CreateDirectory(directory);
      }

      string json = JsonSerializer.Serialize(new AppState(roots), new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(_stateFilePath, json);
    }
    catch (Exception ex)
    {
      Log($"設定の保存に失敗しました: {ex.Message}");
    }
  }

  private void InitializeComponent()
  {
    SuspendLayout();
    // 
    // MainForm
    // 
    ClientSize = new Size(1084, 681);
    Name = "MainForm";
    ResumeLayout(false);

  }

  private static string GetStateFilePath()
  {
    string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    return Path.Combine(baseDir, "ProjectCleaner", "roots.json");
  }

  private sealed record AppState(string[] Roots);
}