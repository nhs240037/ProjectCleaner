-----

# Requirements Document

## Introduction

### 背景

`MainForm.cs` には、スキャン処理中にスキャンルート数に応じた実際の進捗を表示する `UpdateScanProgress` と、進捗が不明な操作中にバーが動くように見える疑似進捗アニメーション（`AdvancePseudoProgress` + `_progressTimer`）が、`MainForm` クラス内のフィールドとメソッドとして混在している。これらは `MainForm` の責務を肥大化させ、将来の再利用（例: 掃除処理やその他の長時間処理にも同じ進捗 UI を表示したい場合）が難しい状態である。

### 目的

疑似プログレスバーを独立した WinForms コンポーネントに抽出し、`MainForm` の責務を分離する。進捗表示のビジュアル部分（トラックとフィll）を `PseudoProgressBar` が管理し、`MainForm` はラベルテキストの更新のみを行うシンプルな構成とする。

### 変更範囲

- フィールド: `_progressTrack`, `_progressFill`, `_progressTimer`, `_progressValue`, `_progressTarget` を削除
- メソッド: `UpdateScanProgress`, `SetScanningProgressVisible`, `AdvancePseudoProgress` を `PseudoProgressBar` に委譲
- 新規: `PseudoProgressBar` コンポーネント

## Requirements

### Requirement 1: 疑似プログレスバーを独立コンポーネントとして抽出

**User Story:** 開発者として、`MainForm` から進捗表示の細部を分離し、再利用可能なコンポーネントとして扱いたい。

#### Acceptance Criteria

1. WHEN `MainForm` がスキャンや掃除などの長時間処理を開始する THEN `MainForm` は進捗表示の内部実装（`Panel`, `Timer`, 幅の計算）を直接扱わず、コンポーネントの公開メソッドを呼ぶ
2. IF 疑似プログレスバーを再利用したい処理（例: 掃除処理）がある THEN 既存の `MainForm` コードを変更せずに同じコンポーネントをインスタンス化して利用できる
3. WHEN コンポーネントが `MainForm` から分離される THEN `MainForm.cs` の進捗関連のフィールドとメソッドが削除され、コードが単純になる

### Requirement 2: 既存の進捗表示の動作を維持

**User Story:** ユーザーとして、現在の UI の動作が変わらないでほしい。

#### Acceptance Criteria

1. WHEN スキャンが開始される THEN 進捗バーが表示され、スキャン中に表示される
2. WHEN スキャンが進捗情報を提供する場合（ルートごとの進捗） THEN バーの幅がその進捗に応じて更新される
3. WHEN 進捗が 0 の場合 THEN `検査中: n/a` と表示される
4. WHEN 処理が完了する THEN 進捗バーが非表示になり、サマリ表示に復元される
5. WHEN 2回目以降のスキャンが行われる THEN プログレスバーが正しく表示される

### Requirement 3: UI/UX の一貫性

**User Story:** ユーザーとして、進捗表示の一貫した見た目を維持してほしい。

#### Acceptance Criteria

1. WHEN コンポーネントが `MainForm` に統合される THEN 既存の `MainForm` と同じスタイル（青いバー、固定3Dのトラック）を維持する
2. WHEN `MainForm` のサイズ変更があった THEN 進捗バーの幅がトラックの幅に応じて再計算される
3. WHEN 進捗テキストが更新される THEN 左側のラベルに `検査中: X/Y` が表示される

## Non-Functional Requirements

### Code Architecture and Modularity

- **Single Responsibility Principle**: 進捗表示は `PseudoProgressBar` がビジュアル部分のみを管理し、`MainForm` がテキスト表示を管理する
- **Modular Design**: `PseudoProgressBar` は UI の表示のみを担当し、スキャンや掃除のビジネスロジックには介入しない
- **Dependency Management**: `MainForm` は `PseudoProgressBar` の公開 API (`SetVisible`, `Start`, `Update`, `HideProgress`) を参照する
- **Clear Interfaces**: コンポーネントは明示的な公開メソッドを持つ

### Performance

- 疑似進捗アニメーションは `System.Windows.Forms.Timer` を使用し、UI スレッド上で動作する
- 進捗の更新は必要最小限の描画のみを行い、UI の応答性を損なわない

### Reliability

- 進捗 target が 0 の場合は `検査中: n/a` と表示し、エラーにならない
- UI スレッド外から進捗が更新される場合は、`InvokeRequired` を確認して UI スレッドにディスパッチする

### Usability

- ユーザーには進捗の進行状況が直感的に伝わる
- 進捗が不明な場合でも、操作が完了するまで表示され、完了後は自然に消える