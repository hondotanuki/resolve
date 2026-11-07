# Implementation Roadmap

アプリ完成までの全体計画を示す。この文書はマイルストーンを管理し、実際に着手する作業は `docs/internal/IMPLEMENTATION_PROGRESS.md` で小さな計画へ分解する。

## 進め方

採用する方式は、**学習駆動の縦切りスライス + 必要時だけのSpike** とする。

- 1つの機能をDomain、Application、Infrastructure、API、Reactまで小さく貫通させる
- 各Issueを小さく切り、実装・必要なUI・テスト・確認を完結させる
- 新しいC# / .NET概念が必要になったら、実装前に30〜90分の小さなSpikeを行う
- Spikeは使い捨ての検証とし、得た判断を必要に応じて記録する
- 画面は各機能のIssueでデザイン・操作性・エラー状態まで仕上げる
- 実装は開発者が行い、Codexは設計相談、学習支援、レビュー、次の小計画の作成を担う

## ドキュメントの役割

| 文書                                       | 役割                                                            |
| ------------------------------------------ | --------------------------------------------------------------- |
| `PROJECT_DESIGN.md`                        | プロダクト仕様と設計判断の正本                                  |
| この文書                                   | アプリ完成までのマイルストーンと順序                            |
| `docs/internal/IMPLEMENTATION_PROGRESS.md` | 現在地、次の小計画、実測結果、学習ログ。Git管理しない個人用文書 |
| `AGENTS.md`                                | 人間とCodexの協働ルール、レビュー方針                           |

## GitHub運用

- GitHub Projectの列は `Backlog / Ready / In Progress / Review / Done` とする
- マイルストーンごとにEpic Issueを作り、実装は子Issueとして管理する
- ブランチは `feat/<issue-number>-<slug>`、修正は `fix/<issue-number>-<slug>` とする
- 原則は1 Issue = 1 Pull Requestとする
- コミットは説明可能な論理的変更単位に分け、Conventional Commits形式を推奨する
- IssueとPRの本文は `.github/ISSUE_TEMPLATE/` と `.github/pull_request_template.md` を正本とする

## Issueの標準形式

開発タスクは `.github/ISSUE_TEMPLATE/task.md` を使用し、目的・実装・受け入れ条件・関連資料を作業範囲に合わせて記載する。

## マイルストーン

### M0: 開発環境と運用基盤

- .NET 10 SDK、Node.js、SQLite、Git、GitHub CLI、VS Code拡張を開発者が導入する
- `backend/Resolve.slnx` と `backend/src`・`backend/tests` に最小4層プロジェクトを作成する。React/Viteは `frontend/` に作成する
- ルートは共通設定・ドキュメント・開発用ツール定義だけを置く。ルートの確認用Consoleプロジェクトは、バックエンドSolutionの検証後に削除する
- `.editorconfig` を追加し、C#にはCSharpierと.NET Analyzers、ReactにはPrettierとOxlintを導入する
- 各ツールの動作確認後に、対応する保存時フォーマット・静的検査を`.vscode/settings.json`へ設定する
- GitHub Project、Issueテンプレート、PRテンプレートを設定する

完了条件: API・Reactを起動でき、導入済みのフォーマット・静的検査・テストの実行コマンドがREADMEに記載されている。

### M1: 最初の縦切り — LearningItemの登録・表示

- `LearningItem`、`ItemType`、登録・一覧・詳細の最小UseCaseを実装する
- `learning_items` Migration、登録・一覧・詳細API、Reactの登録フォーム・一覧・詳細を実装する
- 学習テーマ: C# class/record、nullable reference types、DTO、EF Core Migration、xUnit

完了条件: 手入力した学習対象をDBへ保存し、ブラウザで一覧・詳細を確認できる。

### M2: 競技問題の登録・学習記録・復習

進行中。学習記録の登録・履歴・訂正・削除と現在状態の表示は実装済みだが、新方針の問題情報・タイマー・判定へ変更する。

1. AtCoder・OMCの問題URLから必要な情報を取得できるか、利用条件とあわせてSpikeで調査する。
2. 問題区分とコード内の目標時間、正誤・所要時間・保存時の目標時間に合わせてDomainとデータモデルを見直す。
3. URLからの問題自動登録・重複時の既存詳細表示・登録後の編集、問題情報と履歴を中心とした詳細画面を実装する。
4. 目標時間の通知、一時停止・再開を備えたタイマーと学習記録の登録・訂正を実装する。
5. 時間内正解の連続回数と経過時間による復習推薦、未着手と復習対象を別枠で示すホームを実装する。同じIssueでStudyRecordのLocationに対応する単体GETを追加する。

各Issueで対象画面の見た目・操作性、読み込み・空・エラー状態、必要なテストとブラウザ確認まで行う。M2のIssue案は `docs/internal/m2-plan.md` に置く。

完了条件: AtCoder・OMCの対応問題をURLから登録し、詳細・履歴を確認できる。タイマーで正誤・所要時間・目標時間を記録・訂正でき、履歴に応じた未着手・復習対象をホームで確認できる。必要なテストとブラウザ確認が成功する。

### M3: 問題の整理とアーカイブ

- 問題のアーカイブと、履歴を保持した通常表示からの除外を実装する。
- 問題をまとめるグループの必要性と範囲を、実際の利用を踏まえて検討する。
- 出典・URLの登録と問題編集はM2で扱う。

完了条件: アーカイブした問題が通常一覧・ホームから除外され、学習履歴が保持される。

### M4: 問題を探しやすくする

- 検索・絞り込み・並び替え、固定タグの必要性と対象項目を検討する。
- 採用した機能を画面・API・テストまで実装する。問題区分と別の難易度指標を扱うかは未決定とする。

完了条件: 採用した検索・分類機能を問題一覧から利用できる。具体的な条件は着手時に決める。

### M5: 廃止

旧計画のOpenAIによる候補生成は採用しない。履歴を分かりやすくするため、以降の番号は振り直さない。

### M6: 品質・公開準備

- API・Domain・Reactのテストを整理する
- README、環境構築、Migration、テスト手順を整備する
- Docker化または最小デプロイを行い、ローカル以外で動作確認する

完了条件: 新しい環境でREADMEだけを使って起動・テストできる。

## チェックポイントの進め方

1. 開発者が現在のIssue・ブランチ・変更内容を共有する
2. Codexが設計書との整合、完了条件、次の最小ステップをレビューする
3. 開発者が実装、Formatter、Linter、テストを実行する
4. 開発者が結果・差分・疑問点を共有する
5. Codexがレビューし、`docs/internal/IMPLEMENTATION_PROGRESS.md` の次の小計画を提案する
