# Implementation Progress

この文書は、実装の現在地、直近の作業、確認が必要な判断、学習メモを共有するための進捗台帳である。

- 設計の正本: `docs/PROJECT_DESIGN.md`
- 実装の進捗と次の作業: この文書
- 実装作業は、原則としてこの文書の「次のチェックポイント」を確認してから開始する

## 進め方

各チェックポイントでは、次の順で進める。

1. 前回の完了条件と変更内容を確認する
2. 次の小さな作業の目的と学習テーマを確認する
3. 開発者が実装・テスト・フォーマットを行う
4. 実測結果、学んだこと、次の候補をこの文書へ記録する
5. ユーザーの指示を受けて次のチェックポイントへ進む

## 現在地

| 項目                          | 状態                         |
| ----------------------------- | ---------------------------- |
| ドメイン・UseCase・データ設計 | 完了                         |
| 実装マイルストーン計画        | 完了                         |
| GitHub Project / Issue運用    | 未開始                       |
| C# / .NET 開発環境            | 未開始                       |
| React 開発環境                | Node.js / npm は確認済み     |
| C# API                        | 未開始                       |
| React UI                      | 未開始                       |
| LLM分析                       | 未開始（後半マイルストーン） |

## 環境診断結果

確認日: 2026-09-06

| 項目                   | 結果                         |
| ---------------------- | ---------------------------- |
| OS                     | Ubuntu 22.04.4 LTS           |
| Node.js                | v20.9.0                      |
| npm                    | v10.1.0                      |
| Git                    | 利用可能                     |
| VS Code Remote CLI     | 利用可能                     |
| .NET SDK               | 未導入                       |
| C# / Reactプロジェクト | 未作成                       |
| GitHub CLI             | 導入済み、認証トークンは無効 |

## 次のチェックポイント: CP-00 開発環境を整える

### 目的

C# APIとReact UIを同じワークスペースで開発し、保存時フォーマットとコミット前の静的検査を実行できる状態にする。

### 学習テーマ

- .NET SDK、Solution、プロジェクトの関係
- ASP.NET Core Web APIとEntity Framework Coreの役割
- npmの依存管理
- `.editorconfig`、Formatter、Linterの役割
- VS Codeワークスペース設定

### 作業候補

- [x] .NET 10 SDKを導入し、`dotnet --info` を確認する
- [ ] 有効なGitリポジトリとGitHub remoteを準備する
- [ ] GitHub CLIを再認証し、Issue・Project運用を開始する
- [ ] C# Solutionと最小4層プロジェクトを作成する
- [ ] React / Viteプロジェクトを作成する
- [ ] `.editorconfig` を追加してC#とTypeScriptの基本スタイルを固定する
- [ ] C#はCSharpierと`dotnet format`、ReactはPrettierとESLintを導入する
- [ ] `.vscode/settings.json` に保存時フォーマットとESLint修正を設定する
- [ ] Formatter、Linter、テストを実行するコマンドをREADMEへ記載する

### 完了条件

- `dotnet --info`、`node --version`、`npm --version` が成功する
- C# APIとReact開発サーバーを起動できる
- C#・TypeScriptファイルを保存するとフォーマットされる
- `dotnet format --verify-no-changes`、ESLint、Prettierの検査を実行できる
- GitHub Issueからブランチを作り、最初の環境構築PRを作成できる

### 開発者が行うこと

- .NET SDK、npmパッケージ、VS Code拡張、GitHub CLIの導入・設定は開発者が行う
- 現在の`.git`は有効なGitリポジトリではないため、Git初期化前に既存内容の扱いを確認する

### Codexに依頼すること

- 導入手順、設定内容、コマンド出力、設定ファイルのレビュー
- Formatter、Linter、`.editorconfig`、`.vscode/settings.json` の方針提案
- CP-00完了後の最初のIssueと小計画の作成

## 次の候補

| チェックポイント | 内容                            | 実施条件    |
| ---------------- | ------------------------------- | ----------- |
| CP-01            | LearningItemの登録・一覧・詳細  | CP-00完了後 |
| CP-02            | StudyRecord、状態算出、復習推薦 | CP-01完了後 |
| CP-03            | 出典、グループ、アーカイブ      | CP-02完了後 |
| CP-04            | 固定タグ、手動難易度、検索      | CP-03完了後 |
| CP-05            | OpenAI APIによるLLM候補         | CP-04完了後 |

## 作業ログ

### CP-00 開始前

- 2026-09-06: `PROJECT_DESIGN.md` にC#、固定タグ、手動/LLM難易度、4層構成の方針を反映した。
- 2026-09-06: 環境を診断した。Node.js、npm、Git、VS Code Remote CLIは利用可能で、.NET SDKは未導入だった。
