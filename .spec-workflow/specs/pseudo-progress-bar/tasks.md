-----

# Tasks Document

- [x] 1. `PseudoProgressBar.cs` を新規作成
  - File: `PseudoProgressBar.cs`
  - 進捗バーの表示・非表示、進捗値の更新、疑似進捗アニメーションを管理する WinForms コンポーネントを実装する
  - 既存の `MainForm` と同じスタイル（青いバー、固定3Dのトラック）を維持する
  - Purpose: 進捗表示の責務を `MainForm` から分離する
  - _Leverage: `MainForm.cs` の進捗関連のフィールドとメソッド（`_progressTrack`, `_progressFill`, `_progressTimer`, `_progressValue`, `_progressTarget`, `UpdateScanProgress`, `SetScanningProgressVisible`, `AdvancePseudoProgress`）
  - _Requirements: 1.1, 2.1
  - _Prompt: Role: C# WinForms Developer | Task: `MainForm.cs` から進捗表示の細部を分離し、独立した `PseudoProgressBar` コンポーネントを実装する | Restrictions: 既存の `MainForm` と同じ見た目と挙動を維持すること、進捗の clamp や target 0 のケースを安全に処理すること | Success: `PseudoProgressBar` が単独で進捗表示を完結し、`MainForm` から進捗関連のフィールドとメソッドを削除できる状態になる

- [x] 2. `MainForm.cs` から進捗関連のフィールドとメソッドを削除
  - File: `MainForm.cs`
  - `_progressTrack`, `_progressFill`, `_progressTimer`, `_progressValue`, `_progressTarget` のフィールドを削除する
  - `SetScanningProgressVisible`, `AdvancePseudoProgress` のメソッドを削除し、`PseudoProgressBar` の公開メソッドに置き換える
  - Purpose: `MainForm` の進捗関連の責務を完全に `PseudoProgressBar` に委譲する
  - _Leverage: `PseudoProgressBar.cs`
  - _Requirements: 1.1, 1.3
  - _Prompt: Role: C# WinForms Developer | Task: `MainForm.cs` の進捗関連のフィールドとメソッドを `PseudoProgressBar` の公開メソッド呼び出しに置き換える | Restrictions: 既存の UI 動作（スキャン開始・完了時の進捗表示、バーの非表示、サマリ表示の復元）を維持すること | Success: `MainForm.cs` から進捗関連のコードが削除され、進捗表示はすべて `PseudoProgressBar` を介して行われる状態になる

- [x] 3. `MainForm.cs` に `PseudoProgressBar` を統合
  - File: `MainForm.cs`
  - `MainForm` に `PseudoProgressBar` のインスタンスを追加し、`BuildLayout` で UI に追加する
  - `ScanAsync` など、進捗表示が必要な処理で `PseudoProgressBar` の公開メソッドを呼ぶようにする
  - Purpose: 進捗表示を `MainForm` の操作フローに自然に組み込む
  - _Leverage: `PseudoProgressBar.cs`
  - _Requirements: 1.2, 2.2, 2.3, 2.4
  - _Prompt: Role: C# WinForms Developer | Task: `MainForm` に `PseudoProgressBar` を統合し、スキャンや掃除の開始・完了時に進捗表示を制御する | Restrictions: 進捗表示の開始・完了のタイミングが既存の `MainForm` と同じになること | Success: `MainForm` の操作フローで進捗表示が正しく表示・非表示され、既存の UI 動作が維持される状態になる

- [x] 4. 「検査中 n/a」ラベル表示不具合の修正
  - File: `MainForm.cs`, `PseudoProgressBar.cs`
  - `UpdateScanProgress` で total が 0 の場合に `検査中: n/a` と表示されるようにする
  - Purpose: 進捗不明時に適切なラベルを表示する
  - _Leverage: `PseudoProgressBar.cs`, `MainForm.cs`
  - _Requirements: 2.3
  - _Prompt: Role: C# WinForms Developer | Task: 進捗不明時のラベル表示を実装する | Restrictions: 既存の UI 動作を壊さないこと | Success: target が 0 の場合に「検査中: n/a」が表示される状態になる

- [x] 5. 2回目以降のスキャンでプログレスバーが出ない不具合の修正
  - File: `MainForm.cs`
  - `_pseudoProgressBar.Visible = true` を `_pseudoProgressBar.SetVisible(true)` に変更し、内部のトラック表示を正しく制御する
  - Purpose: 複数回のスキャンで正しくプログレスバーが表示されるようにする
  - _Leverage: `PseudoProgressBar.cs`
  - _Requirements: 2.5
  - _Prompt: Role: C# WinForms Developer | Task: 2回目以降のスキャンでプログレスバーが表示されない問題を修正する | Restrictions: 既存の UI 動作を壊さないこと | Success: 2回目以降のスキャンでもプログレスバーが正しく表示される状態になる

- [x] 6. 進捗表示の動作確認
  - File: `MainForm.cs`, `PseudoProgressBar.cs`
  - スキャン開始時に進捗バーが表示され、スキャンの進捗に応じて幅が更新されることを確認する
  - 進捗情報が不明な場合でも「検査中: n/a」が表示されることを確認する
  - 処理完了時に進捗バーが非表示になり、サマリ表示に復元されることを確認する
  - 2回目以降のスキャンでも正しく動作することを確認する
  - Purpose: 進捗表示の品質を保証する
  - _Leverage: `PseudoProgressBar.cs`, `MainForm.cs`
  - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5
  - _Prompt: Role: QA Engineer with expertise in WinForms UI | Task: 進捗表示の動作をエンドツーエンドで確認する | Restrictions: 既存の UI 動作を壊さないこと | Success: 進捗表示がすべての操作フローで正しく動作し、既存の UI 動作が維持される状態になる

- [x] 7. ビルド検証
  - Command: `dotnet build ProjectCleaner.csproj --no-restore -c Release`
  - Purpose: コンパイルエラーがないことを確認する
  - _Requirements: All
  - _Prompt: Role: DevOps Engineer | Task: リリースビルドでコンパイルエラーがないことを確認する | Restrictions: 既存のビルド設定を変更しないこと | Success: ビルドが成功し、エラー・警告なし（既存の警告除く）の状態になる