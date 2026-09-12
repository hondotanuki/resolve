# Implementation Roadmap

アプリ完成までの全体計画を示す。この文書はマイルストーンを管理し、実際に着手する作業は `docs/internal/IMPLEMENTATION_PROGRESS.md` で小さな計画へ分解する。

## 進め方

採用する方式は、**学習駆動の縦切りスライス + 必要時だけのSpike** とする。

- 1つの機能をDomain、Application、Infrastructure、API、Reactまで小さく貫通させる
- 各Issueは「学習目標 → 最小実装 → テスト → 振り返り」で完結させる
- 新しいC# / .NET概念が必要になったら、実装前に30〜90分の小さなSpikeを行う
- Spikeは使い捨ての検証とし、得た判断だけをIssueの振り返りへ残す
- 実装は開発者が行い、Codexは設計相談、学習支援、レビュー、次の小計画の作成を担う

## ドキュメントの役割

| 文書 | 役割 |
| --- | --- |
| `PROJECT_DESIGN.md` | プロダクト仕様と設計判断の正本 |
| この文書 | アプリ完成までのマイルストーンと順序 |
| `docs/internal/IMPLEMENTATION_PROGRESS.md` | 現在地、次の小計画、実測結果、学習ログ。Git管理しない個人用文書 |
| `AGENTS.md` | 人間とCodexの協働ルール、レビュー方針 |

## GitHub運用

- GitHub Projectの列は `Backlog / Ready / In Progress / Review / Done` とする
- マイルストーンごとにEpic Issueを作り、実装は子Issueとして管理する
- ブランチは `feat/<issue-number>-<slug>`、修正は `fix/<issue-number>-<slug>` とする
- 原則は1 Issue = 1 Pull Requestとする
- コミットは説明可能な論理的変更単位に分け、Conventional Commits形式を推奨する
- PRには関連Issue、設計判断、テスト結果、学習上の気づき、未対応事項を記載する

## Issueの標準形式

```markdown
## 目的

このIssueでユーザーに提供する価値。

## 学ぶこと

C# / ASP.NET Core / EF Core / React の具体的な学習テーマ。

## Spike（必要な場合のみ）

- 検証したい仮説
- 実験方法
- 採用する判断

## 実装

- [ ] Domain
- [ ] Application
- [ ] Infrastructure
- [ ] API
- [ ] React

## 受け入れ条件

- [ ] ユーザー操作として確認できる結果
- [ ] 正常系・境界値・異常系
- [ ] 自動テスト

## 振り返り

- 学んだこと
- 詰まったこと
- 次回改善すること
```

## マイルストーン

### M0: 開発環境と運用基盤

- .NET 10 SDK、Node.js、SQLite、Git、GitHub CLI、VS Code拡張を開発者が導入する
- Solution、最小4層プロジェクト、React/Vite、SQLite接続、ヘルスチェックを作成する
- `.editorconfig`、CSharpier、`dotnet format`、Prettier、ESLint、`.vscode/settings.json` を導入する
- GitHub Project、Issueテンプレート、PRテンプレートを設定する

完了条件: API・Reactを起動でき、フォーマット、Lint、テストの実行コマンドがREADMEに記載されている。

### M1: 最初の縦切り — LearningItemの登録・表示

- `LearningItem`、`ItemType`、登録・一覧・詳細の最小UseCaseを実装する
- `learning_items` Migration、登録・一覧・詳細API、Reactの登録フォーム・一覧・詳細を実装する
- 学習テーマ: C# class/record、nullable reference types、DTO、EF Core Migration、xUnit

完了条件: 手入力した学習対象をDBへ保存し、ブラウザで一覧・詳細を確認できる。

### M2: 学習記録・状態・復習推薦

- `StudyRecord`、`StudyResult`、`ItemStatusCalculator`、`ReviewSelector` を実装する
- 学習記録の登録・訂正・削除・履歴表示と、`GET /review-items` を追加する
- 学習テーマ: Value Object、Enum、Domain Service、LINQ、単体テスト

完了条件: 学習記録の変更に応じて状態・復習対象が正しく変わる。

### M3: 出典・グループ・アーカイブ

- `Source`、`ItemReference`、`Collection`、`ItemCollection` を実装する
- 学習対象の編集、出典・URL・グループ関連付け、アーカイブを追加する
- 学習テーマ: Entity関連、EF Core Fluent API、Transaction、Problem Details

完了条件: AtCoder問題と英単語帳の項目を同じUI・APIで登録でき、アーカイブ済み項目は通常一覧から除外される。

### M4: 固定タグ・手動難易度・検索

- `TagCode` enumとTagDefinitionを定義し、EF Core Seed Dataとして`tags`を用意する
- `ItemTag`、`ItemEvaluation`、手動難易度設定、検索・絞り込みを実装する
- 学習テーマ: Seed Data、制約、検索クエリ、API統合テスト

完了条件: 無効なタグコードを拒否し、確定済みタグ・難易度で一覧を絞り込める。

### M5: OpenAI APIによるLLM候補

- `PromptSelector`、`AnalysisResultValidator`、OpenAI Client Interfaceを実装する
- 構造化出力から難易度・固定タグ候補を保存し、採用・修正・却下を実装する
- 学習テーマ: Interface、DI、async/await、構造化出力、外部サービスのモック

完了条件: カタログ外タグを保存せず、確定難易度・タグだけを通常表示と検索に使える。

### M6: 品質・公開準備

- API・Domain・Reactのテストを整理する
- README、環境構築、Migration、テスト、OpenAI設定を整備する
- Docker化または最小デプロイを行い、ローカル以外で動作確認する

完了条件: 新しい環境でREADMEだけを使って起動・テストできる。

## チェックポイントの進め方

1. 開発者が現在のIssue・ブランチ・変更内容を共有する
2. Codexが設計書との整合、完了条件、次の最小ステップをレビューする
3. 開発者が実装、Formatter、Linter、テストを実行する
4. 開発者が結果・差分・疑問点を共有する
5. Codexがレビューし、`docs/internal/IMPLEMENTATION_PROGRESS.md` の次の小計画を提案する
