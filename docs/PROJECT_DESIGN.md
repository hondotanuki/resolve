# 1. Overview

## 1.1 目的

- 競技プログラミング、数学、英語などの学習対象を一元管理する
- 学習結果と履歴を記録し、次に復習すべき対象を把握する
- URL、教材、手入力のいずれの学習対象も扱えるようにする
- LLMを使って難易度と固定タグの候補を生成し、ユーザーが確定する

競技プログラミングの問題は、このアプリで扱う `LearningItem` の一種である。

## 1.2 対象ユーザー

- 個人利用
- ローカル利用を基本とし、将来的なWeb利用も可能にする

## 1.3 開発目的

開発者はPythonの知識を土台に、C#を学びながらAI Agentも活用してアプリを完成させる。

- アプリ開発を通してC#、React、デプロイ、DDD、Clean Architectureの知識を深める
- 学習対象を継続して管理できるアプリを完成させる

### バックエンド

- C#での設計力を高める
- ASP.NET CoreとEntity Framework Coreを理解する
- DB操作、型、例外処理、テスト、DIを学ぶ
- LLMの構造化出力をアプリケーションに組み込む

### フロントエンド

- ReactとTypeScriptで一覧・詳細・登録画面を実装する

---

# 2. Scope

## 2.1 MVPで実装するもの

### 学習対象

- 学習対象の登録、一覧、詳細、編集、アーカイブ
- 出典、URL、グループ、固定タグの関連付け
- 状態、出典、グループ、タグ、難易度による絞り込み

### 学習記録

- 学習記録の登録、一覧、訂正、削除
- 現在状態の表示
- 復習対象の推薦

### 難易度・タグ

- 難易度を手動入力またはLLM分析から選択する
- LLMが固定タグカタログからタグ候補を選択する
- LLMの難易度・タグ候補をユーザーが採用、修正、却下する

## 2.2 MVPでは実装しないもの

- ユーザー認証と複数ユーザー対応
- AtCoderなどの外部API連携、自動取得
- 任意のユーザー定義項目・ユーザー定義タグ
- LLMの生レスポンスの永続化、詳細な実行監査ログ
- LLM結果の自動確定

---

# 3. Use Cases

## 3.1 入力とテーブルの対応方針

UseCaseの入力は、テーブル列をそのまま公開するものではなく、ユーザーの操作に必要な**コマンド**である。

- 登録・更新する値は、原則としてEntityまたは関連Entityの属性に対応する
- `id`、`created_at`、`updated_at`、現在状態など、システムが決める値は入力に含めない
- `LearningItemId` や確認操作など、既存データの特定・操作のための入力はテーブルの業務属性ではない
- 出典、グループ、タグのような関連は、ネストした入力としてまとめて受け取り、関連テーブルに保存する
- 検索条件や並び順は読取用の入力であり、テーブルへの保存対象ではない

たとえば、`UC-I01` の `title` は `learning_items.title` に、タグコードは `item_tags.tag_id` に、手動難易度は `item_evaluations` に対応する。一方、`difficulty_mode` は処理を選ぶための入力であり、直接保存する列ではない。

## UC-I01 学習対象を登録する

### 目的

新しく学ぶ対象を管理対象に追加する。

### 入力

#### 基本情報 → learning_items

- タイトル
- 学習対象種別
- 本文または補足情報（任意）

#### 出典情報 → item_references（任意）

- 出典スラッグ
- 出典内の識別子
- URL
- 出典上の表示名

#### グループ情報 → collections / item_collections（任意）

- グループ名
- グループ種別
- グループ内の位置またはラベル

#### タグ → item_tags（任意）

- タグコードの配列

#### 難易度 → item_evaluations（任意）

- 入力方式: `manual` / `llm` / `none`
- 手動入力時のみ: 難易度の値、尺度

### 処理

1. システムが入力値とタグコードを検証する
2. LearningItemを保存する
3. 入力された出典、グループ、確定済みタグを保存または関連付ける
4. 入力方式が `manual` の場合、確定済み難易度を保存する
5. 入力方式が `llm` の場合、UC-L01を実行して候補を生成する

### 結果

学習対象が一覧と詳細画面に表示される。LLM方式を選んだ場合、難易度とタグは候補として表示される。

### エラーケース

- タイトルまたは学習対象種別が未入力
- URLが不正
- 同一出典内の識別子が既存の参照と重複する
- 存在しない、または無効化されたタグコードが指定された
- 手動難易度の値または尺度が不正
- LLM分析に必要な本文がない

---

## UC-I02 学習対象一覧を確認する

### 目的

登録済みの学習対象を一覧で確認する。

### 入力

- 検索語（任意）
- 学習対象種別（任意）
- 出典（任意）
- グループ（任意）
- タグコード（任意）
- 確定済み難易度の範囲（任意）
- 現在状態（任意）
- 並び順（任意）

### 処理

1. 条件に一致する、アーカイブされていないLearningItemを取得する
2. 現在状態を学習記録から算出する
3. 確定済みのタグと難易度を取得する
4. 一覧を表示する

### 結果

タイトル、種別、出典、グループ、確定済み難易度、確定済みタグ、現在状態、最終学習日時が表示される。

### エラーケース

- 検索条件が不正
- 学習対象または関連情報の取得に失敗する

---

## UC-I03 学習対象詳細を確認する

### 目的

学習対象の内容、評価、タグ、学習履歴を確認する。

### 入力

- 学習対象ID

### 処理

1. 指定されたLearningItemを取得する
2. 出典、グループ、確定済み・候補タグ、難易度を取得する
3. 学習記録を取得する
4. 現在状態を算出する
5. 詳細画面を表示する

### 結果

基本情報、URL、出典、グループ、タグ、難易度、現在状態、学習履歴が表示される。

### エラーケース

- 指定された学習対象が存在しない
- 関連情報または学習記録の取得に失敗する

---

## UC-I04 学習対象を編集する

### 目的

登録済みの学習対象と関連情報を修正する。

### 入力

- 学習対象ID
- UC-I01の基本情報、出典情報、グループ情報、確定済みタグ

### 処理

1. 指定されたLearningItemを取得する
2. システムが入力値とタグコードを検証する
3. LearningItemと関連情報を更新する

### 結果

一覧と詳細画面に更新後の情報が表示される。

### エラーケース

- 指定された学習対象が存在しない
- タイトルが未入力
- URLが不正
- 出典内の識別子が重複する
- 無効なタグコードが指定された

---

## UC-I05 学習対象をアーカイブする

### 目的

不要になった学習対象を通常の一覧から非表示にする。

### 入力

- 学習対象ID
- アーカイブ確認

### 処理

1. 指定されたLearningItemを取得する
2. ユーザーに確認を求める
3. `archived_at` を設定する

### 結果

通常の一覧と復習推薦から除外され、学習履歴は保持される。

### エラーケース

- 指定された学習対象が存在しない
- アーカイブ確認が行われていない

---

## UC-R01 学習記録を登録する

### 目的

1回の学習結果を履歴として保存する。

### 入力 → study_records

- 学習対象ID
- 学習日時
- 結果: `success` / `failure` / `reviewed`
- 所要時間（任意）
- メモ（任意）

### 処理

1. 対象のLearningItemを取得する
2. 入力値を検証する
3. StudyRecordを保存する
4. 現在状態を再計算する

### 結果

学習記録が詳細画面の履歴に追加される。

### エラーケース

- 対象の学習対象が存在しない
- 学習日時または結果が不正
- 所要時間が負の値である

---

## UC-R02 学習履歴を確認する

### 目的

学習対象に対する過去の学習記録を確認する。

### 入力

- 学習対象ID

### 処理

1. 指定されたLearningItemを取得する
2. 紐づくStudyRecordを学習日時の新しい順に取得する
3. 履歴を表示する

### 結果

学習日時、結果、所要時間、メモが表示される。

### エラーケース

- 指定された学習対象が存在しない
- 学習記録の取得に失敗する

---

## UC-R03 学習記録を訂正する

### 目的

入力ミスのある学習記録を修正する。

### 入力 → study_records

- 学習記録ID
- 学習日時
- 結果
- 所要時間（任意）
- メモ（任意）

### 処理

1. 指定されたStudyRecordを取得する
2. 入力値を検証する
3. StudyRecordを更新する
4. 現在状態を再計算する

### 結果

修正後の記録と現在状態が表示される。

### エラーケース

- 指定された学習記録が存在しない
- 入力値が不正

---

## UC-R04 学習記録を削除する

### 目的

誤って登録した学習記録を削除する。

### 入力

- 学習記録ID
- 削除確認

### 処理

1. 指定されたStudyRecordを取得する
2. ユーザーに確認を求める
3. StudyRecordを削除する
4. 現在状態を再計算する

### 結果

削除した記録は履歴に表示されなくなる。

### エラーケース

- 指定された学習記録が存在しない
- 削除確認が行われていない

---

## UC-D01 手動で難易度を設定する

### 目的

ユーザーが学習対象の難易度を直接設定または変更する。

### 入力 → item_evaluations

- 学習対象ID
- 難易度の値
- 難易度の尺度

### 処理

1. 対象のLearningItemを取得する
2. 入力値を検証する
3. 既存の確定済み難易度を置き換える
4. `method = manual`、`status = confirmed` のItemEvaluationを保存する

### 結果

一覧と詳細画面に手動設定した難易度が表示される。

### エラーケース

- 指定された学習対象が存在しない
- 難易度の値または尺度が不正

---

## UC-L01 LLMで難易度とタグ候補を生成する

### 目的

学習対象の本文から難易度と、固定タグカタログに含まれるタグ候補を生成する。

### 入力

- 学習対象ID

### 処理

1. LearningItemの本文と出典情報を取得する
2. 出典と学習対象種別に対応するアプリ内プロンプトを選択する
3. 有効なタグコード一覧をプロンプトに渡す
4. LLMに難易度とタグコードの構造化出力を要求する
5. 出力されたタグコードが有効なTagCodeか検証する
6. 難易度とタグ候補を `suggested` として保存する

### 結果

難易度と固定タグの候補が詳細画面で確認できる。

### エラーケース

- 分析に必要な本文がない
- 対応するプロンプトがない
- LLMの構造化出力が検証に失敗する
- LLMがタグカタログ外のコードを返す
- LLM呼び出しまたは保存に失敗する

---

## UC-L02 LLMの候補を確認する

### 目的

LLMが生成した難易度とタグ候補を採用、修正、または却下する。

### 入力

- 難易度候補IDまたはタグ候補ID
- 操作: `confirm` / `replace` / `reject`
- 修正後の難易度またはタグコード（`replace` 時のみ）

### 処理

1. 未確認の候補を取得する
2. ユーザーの操作を検証する
3. 採用時は `confirmed`、却下時は `rejected` に更新する
4. 修正時は有効なタグコードまたは難易度を `confirmed` として保存する
5. 難易度を確定する場合、既存の確定済み難易度を置き換える

### 結果

採用済みの難易度とタグが一覧・詳細・検索に利用される。

### エラーケース

- 指定された候補が存在しない
- すでに確定または却下済みの候補を操作しようとした
- タグカタログ外のコードを指定した
- 修正値が不正

---

## UC-S01 学習対象を検索・絞り込みする

### 目的

条件に一致する学習対象を探す。

### 入力

- UC-I02の検索条件

### 処理

1. 検索条件を検証する
2. LearningItemと関連情報を検索する
3. StudyRecordから現在状態を算出する
4. 一致する対象を表示する

### 結果

条件に一致する学習対象の一覧が表示される。

### エラーケース

- 検索条件が不正
- データの取得に失敗する

---

## UC-S02 復習対象を確認する

### 目的

復習が必要な学習対象を確認する。

### 入力

- なし

### 処理

1. アーカイブされていないLearningItemを取得する
2. 最新のStudyRecordを取得する
3. 復習ルールに基づいて対象を判定する
4. 復習対象を表示する

### 結果

復習が必要な学習対象の一覧が表示される。

### エラーケース

- 学習対象または学習記録の取得に失敗する

---

# 4. Domain Model

Domain Modelは、DBテーブルの写しではない。Entityは識別子とライフサイクルを持つ概念、Value ObjectとDomain Serviceは値の制約・計算・判断を表す。Data Modelは、それらをEF Coreでどう永続化するかを記述する。

## 4.1 Entities

| Entity | 責務 |
| --- | --- |
| LearningItem | 学習対象と、そのアーカイブ状態を管理する |
| Source | 出典サービスまたは教材提供元を表す |
| ItemReference | LearningItemと出典上の情報を結び付ける |
| Collection | コンテスト、レッスン、章、コースなどのグループを表す |
| Tag | アプリが認める固定タグを表す |
| ItemTag | タグ候補または確定済みのタグ付与を表す |
| ItemEvaluation | 手動またはLLMによる難易度の候補・確定値を表す |
| StudyRecord | 1回の学習行動を表す |

### LearningItem

| 属性 | 意味 |
| --- | --- |
| Id | 一意な識別子 |
| ItemType | 学習対象種別 |
| Title | 表示名 |
| Content | LLM分析に利用できる本文または補足情報 |
| ArchivedAt | アーカイブ日時 |

例: `algorithm_problem`、`english_word`、`english_article`。

### Source / ItemReference

Sourceは `AtCoder`、`OMC`、`英単語帳` などの出典を表す。ItemReferenceは、URL、出典上の表示名、出典内の任意の識別子を持つ。手入力のLearningItemはItemReferenceを持たなくてよい。

### Collection

Collectionはコンテスト、レッスン、章、コース、書籍、問題セットなどを表す。`Kind` は固定enumにせず、未知の種別も文字列として保存できる。LearningItemとは多対多で関連付け、グループ内の位置・ラベルを持てる。

## 4.2 Value Objects and Enums

| 値オブジェクトまたはEnum | 役割 |
| --- | --- |
| `TagCode` | アプリが許可する固定タグのコード。C# enumを正とする |
| `TagStatus` | `Suggested` / `Confirmed` / `Rejected` |
| `DifficultyMethod` | `Manual` / `Llm` |
| `EvaluationStatus` | `Suggested` / `Confirmed` / `Rejected` |
| `StudyResult` | `Success` / `Failure` / `Reviewed` |
| `ItemStatus` | `Unstarted` / `NeedsReview` / `Completed`。保存せず算出する |
| `Difficulty` | 値と尺度を組にし、尺度に応じた値の妥当性を検証する |

TagCodeの例は、競技プログラミング用の `algorithm`、`data_structure`、英語用の `vocabulary`、`grammar`、`reading` のように定義する。実際に許可する一覧はC# enumとシードデータで管理する。

## 4.3 Domain Services

| Service | 責務 |
| --- | --- |
| `ItemStatusCalculator` | 最新のStudyRecordからItemStatusを算出する |
| `ReviewSelector` | 復習ルールに従って復習対象を選ぶ |
| `PromptSelector` | SourceとItemTypeからLLMプロンプトを選ぶ |
| `AnalysisResultValidator` | LLMの構造化出力とTagCodeを検証する |
| `DifficultyConfirmationService` | 1件だけの確定済み難易度を保つ |

---

# 5. Business Rules

## BR-01 アーカイブ

`ArchivedAt` が設定されたLearningItemは、通常の一覧、検索結果、復習推薦から除外する。履歴と関連情報は削除しない。

## BR-02 現在状態

LearningItemの現在状態はDBに保持せず、最新のStudyRecordから算出する。

| 条件 | 現在状態 |
| --- | --- |
| StudyRecordがない | `Unstarted` |
| 最新のResultが`Failure` | `NeedsReview` |
| 最新のResultが`Success`または`Reviewed` | `Completed` |

## BR-03 復習推薦

次のいずれかを満たす、アーカイブされていないLearningItemを復習対象とする。

- 現在状態が `NeedsReview`
- 最新のStudyRecordから7日以上経過している

復習間隔の7日はアプリ設定として管理し、MVPでは固定値とする。

## BR-04 難易度

- 難易度は手動入力またはLLM候補の確定で設定する
- LLMが生成した難易度は必ず `Suggested` として保存する
- 1つのLearningItemに確定済みの `difficulty` は1件だけとする
- 手動入力またはLLM候補の確定は、既存の確定済み難易度を置き換える
- `Confirmed` の難易度だけを一覧表示と検索に利用する

## BR-05 固定タグとLLM

- TagCodeはC# enumを正とし、`tags` テーブルへシードする
- ユーザーは有効なTagCodeだけを選択できる
- LLMには有効なTagCode一覧を渡し、その中からだけ候補を返すよう要求する
- LLMがカタログ外のコードを返した場合、出力は無効として保存しない
- すでに `Confirmed` のタグをLLMが返した場合は、新しい候補を作らず既存のタグを保持する
- `Confirmed` のItemTagだけを通常表示と検索に利用する

## BR-06 プロンプト

- プロンプト本文はアプリケーションの設定ファイルで管理する
- プロンプトは `Source.Slug` と `ItemType` の組み合わせで選択する
- 完全一致するプロンプトがない場合は、`ItemType` の共通プロンプトを使用する
- 使用した `PromptKey` と `PromptVersion` をItemEvaluationに保存する

---

# 6. Data Model

## 6.1 Tables

### learning_items

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| item_type | text | No | 学習対象種別 |
| title | text | No | 表示名 |
| content | text | Yes | 本文または補足情報 |
| archived_at | datetime | Yes | アーカイブ日時 |
| created_at | datetime | No | 登録日時 |
| updated_at | datetime | No | 更新日時 |

### sources

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| name | text | No | 表示名 |
| slug | text | No | 安定した識別子 |

### item_references

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| item_id | integer | No | FK -> learning_items.id |
| source_id | integer | No | FK -> sources.id |
| source_item_key | text | Yes | 出典内の任意の識別子 |
| url | text | Yes | 出典ページURL |
| label | text | Yes | 出典上の表示名 |

### collections

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| source_id | integer | Yes | FK -> sources.id |
| name | text | No | グループ名 |
| kind | text | Yes | グループ種別 |
| description | text | Yes | 補足 |

### item_collections

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| item_id | integer | No | FK -> learning_items.id |
| collection_id | integer | No | FK -> collections.id |
| position | integer | Yes | グループ内の順序 |
| label | text | Yes | グループ内の表示ラベル |

### tags

TagCode enumからEF Coreのシードデータとして投入する固定マスタである。画面・API・LLMは `code` を利用する。

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| code | text | No | C# TagCodeに対応する一意なコード |
| name | text | No | 表示名 |
| is_active | boolean | No | 選択・提案を許可するか |

### item_tags

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| item_id | integer | No | FK -> learning_items.id |
| tag_id | integer | No | FK -> tags.id |
| proposed_by | text | No | `manual` / `llm` |
| status | text | No | `suggested` / `confirmed` / `rejected` |
| created_at | datetime | No | 作成日時 |

### item_evaluations

難易度は `metric = difficulty` で保存する。将来、CEFRなど別の評価を扱う場合にも同じテーブルを利用できる。

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| item_id | integer | No | FK -> learning_items.id |
| metric | text | No | MVPでは `difficulty` |
| value | text | No | 評価値 |
| scale | text | No | 例: `1-5` / `CEFR` |
| method | text | No | `manual` / `llm` |
| status | text | No | `suggested` / `confirmed` / `rejected` |
| prompt_key | text | Yes | LLM利用時のプロンプト識別子 |
| prompt_version | integer | Yes | LLM利用時のプロンプト版 |
| created_at | datetime | No | 作成日時 |

### study_records

| Column | Type | Null | Description |
| --- | --- | --- | --- |
| id | integer | No | Primary Key |
| item_id | integer | No | FK -> learning_items.id |
| studied_at | datetime | No | 学習日時 |
| result | text | No | `success` / `failure` / `reviewed` |
| duration_seconds | integer | Yes | 所要時間（秒） |
| memo | text | Yes | メモ |
| created_at | datetime | No | 登録日時 |
| updated_at | datetime | No | 更新日時 |

## 6.2 Relationships

```text
LearningItem 1 --- N ItemReference --- 1 Source
LearningItem N --- N Collection
LearningItem N --- N Tag
LearningItem 1 --- N ItemEvaluation
LearningItem 1 --- N StudyRecord
```

## 6.3 Constraints

- `sources.slug` は一意
- `item_references` の `(source_id, source_item_key)` は、`source_item_key` が存在する場合に一意
- `item_collections` の `(item_id, collection_id)` は一意
- `tags.code` と `tags.name` は一意
- `item_tags` の `(item_id, tag_id)` は一意
- `item_evaluations` は `status = confirmed` かつ `metric = difficulty` の行を、LearningItemごとに最大1件にする
- `duration_seconds` は0以上
- `result`、`method`、`status`、`proposed_by` はC# enumに定義された値だけを許可する

---

# 7. Screen Design

## 7.1 画面一覧

| 画面 | 目的 |
| --- | --- |
| 学習対象一覧 | 登録済みの学習対象を検索・確認する |
| 学習対象詳細 | 内容、評価、タグ、学習履歴を確認・編集する |
| 学習対象登録・編集 | 学習対象と関連情報を入力する |
| LLM候補確認 | 難易度と固定タグ候補を採用・修正・却下する |

## 7.2 学習対象一覧

### 表示するもの

- タイトル
- 種別
- 出典
- 確定済み難易度
- 確定済みタグ
- 現在状態
- 最終学習日

### 操作

- 詳細を開く
- 検索・絞り込み・並び替え
- 学習対象を追加する
- 復習対象だけを表示する

```text
+----------------------------------------------------------------+
| Learning items                                      [Add]       |
+----------------------------------------------------------------+
| Filter: [Type] [Source] [Collection] [Tag] [State] [Difficulty]|
+----------------------------------------------------------------+
| Title          Type               State         Last studied   |
| ABC300 A       algorithm_problem  needs_review  2026-09-01     |
| ubiquitous     english_word       completed     2026-09-03     |
+----------------------------------------------------------------+
```

---

# 8. API Design

## 8.1 API一覧

| Method | Path | Purpose |
| --- | --- | --- |
| GET | /learning-items | 一覧・検索取得 |
| POST | /learning-items | 学習対象登録 |
| GET | /learning-items/{id} | 詳細取得 |
| PATCH | /learning-items/{id} | 学習対象更新 |
| DELETE | /learning-items/{id} | アーカイブ |
| GET | /learning-items/{id}/study-records | 学習履歴取得 |
| POST | /learning-items/{id}/study-records | 学習記録登録 |
| PATCH | /study-records/{id} | 学習記録訂正 |
| DELETE | /study-records/{id} | 学習記録削除 |
| PUT | /learning-items/{id}/difficulty | 手動難易度設定 |
| POST | /learning-items/{id}/llm-analysis | LLM候補生成 |
| PATCH | /item-evaluations/{id} | 難易度候補の確認 |
| PATCH | /item-tags/{id} | タグ候補の確認 |
| GET | /review-items | 復習対象取得 |

## 8.2 POST /learning-items

### Request

```json
{
  "itemType": "algorithm_problem",
  "title": "ABC300 A",
  "content": "問題文をここに保存する",
  "references": [
    {
      "sourceSlug": "atcoder",
      "sourceItemKey": "abc300_a",
      "url": "https://atcoder.jp/contests/abc300/tasks/abc300_a",
      "label": "ABC300 A"
    }
  ],
  "tagCodes": ["implementation"],
  "difficulty": {
    "mode": "llm"
  }
}
```

### Response

```json
{
  "id": 1,
  "itemType": "algorithm_problem",
  "title": "ABC300 A",
  "archivedAt": null
}
```

### Validation

- `itemType` と `title` は必須
- URLが指定される場合はURL形式を検証する
- `sourceItemKey` が指定される場合、同一Source内で重複してはならない
- `tagCodes` は有効なTagCodeだけを受け付ける
- `difficulty.mode` が `manual` の場合、値と尺度は必須
- `difficulty.mode` が `llm` の場合、`content` は必須

## 8.3 PUT /learning-items/{id}/difficulty

### Request

```json
{
  "value": "3",
  "scale": "1-5"
}
```

このAPIは `method = manual`、`status = confirmed` の難易度を保存し、既存の確定済み難易度を置き換える。

## 8.4 POST /learning-items/{id}/llm-analysis

### Response

```json
{
  "difficulty": {
    "id": 10,
    "value": "3",
    "scale": "1-5",
    "method": "llm",
    "status": "suggested",
    "promptKey": "atcoder_difficulty",
    "promptVersion": 1
  },
  "tags": [
    {"id": 20, "code": "implementation", "status": "suggested"}
  ]
}
```

---

# 9. Backend Design

## 9.1 責務

### API Layer

- ASP.NET Core ControllerまたはMinimal APIでHTTP request / responseを扱う
- Request DTOの入力検証とHTTP status codeへの変換を行う

### Application Layer

- UseCaseを実行する
- Command DTOをDomain Modelに変換する
- トランザクション境界を管理する

### Domain Layer

- Entity、Value Object、Enum、Domain Serviceを置く
- 現在状態、復習対象、難易度確定、LLM出力の妥当性を判断する
- EF CoreやHTTPに依存しない

### Infrastructure Layer

- EF CoreによるDBの保存・取得
- LLM Clientの実装
- Prompt Configurationの読込み

### Prompt Configuration

- 出典と学習対象種別ごとのプロンプト本文をコードで管理する
- `PromptKey` と `PromptVersion` を公開する
- 有効なTagCode一覧をプロンプトへ渡す
- DBにはプロンプト本文もLLMの生レスポンスも保存しない

## 9.2 Dependency Direction

```text
React
  ↓
ASP.NET Core API
  ↓
Application
  ↓
Domain
  ↑
Infrastructure (EF Core / SQLite / LLM Client / Prompt Configuration)
```

## 9.3 Repository Layout

リポジトリ直下は共通設定、ドキュメント、開発用ツール定義を置く場所とし、アプリケーションプロジェクトは置かない。バックエンドとフロントエンドは独立した技術基盤として、それぞれ `backend/` と `frontend/` に配置する。

```text
resolve/
├── backend/
│   ├── Resolve.sln
│   ├── src/
│   │   ├── Resolve.Domain/
│   │   ├── Resolve.Application/
│   │   ├── Resolve.Infrastructure/
│   │   └── Resolve.Api/
│   └── tests/
│       └── Resolve.Domain.Tests/
├── frontend/
├── docs/
├── .vscode/
├── .config/dotnet-tools.json
├── .editorconfig
├── README.md
└── AGENTS.md
```

`backend/Resolve.sln` はバックエンドのプロジェクトだけを管理する。ルートに一時的に作成した Console プロジェクトは、`backend/` の最小 Solution がビルドできたことを確認してから削除する。内容を移植する必要はない。

---

# 10. Technology Stack

## Frontend

- React
- TypeScript
- Vite

## Backend

- C#
- ASP.NET Core Web API
- .NETの標準DIコンテナ

## Database / ORM

- SQLite
- Entity Framework Core

MVPでは通常のWeb APIとして実装し、LLM呼び出しなどのI/Oは `async` / `await` を用いる。高度なキューやバックグラウンドジョブは必要になった時点で導入する。

## Testing

- xUnit
- FluentAssertions
- ASP.NET Coreの統合テスト

---

# 11. C# Learning Goals

## 必ず学ぶ

- class、record、interface
- nullable reference types
- enum、Value Object、Entityの表現
- LINQ
- async / await
- 例外処理
- ASP.NET CoreのDI
- Entity Framework CoreのEntity、Migration、Transaction
- xUnitによるテスト

## 必要になったら学ぶ

- MediatRなどのMediator
- BackgroundService
- OpenTelemetry
- 認証・認可

---

# 12. Testing Strategy

## Unit Test

- StudyRecordがなければ `Unstarted` になる
- 最新のStudyRecordが `Failure` なら `NeedsReview` になる
- 最終学習日から7日経過した対象が復習対象になる
- アーカイブ済み対象が一覧と復習対象から除外される
- 手動難易度の設定で既存の確定済み難易度が置き換わる
- LLM候補が `Suggested` で保存される
- TagCode外のLLM出力が拒否される
- Confirmedのタグ・難易度だけが検索対象になる
- SourceとItemTypeから正しいプロンプトが選択される

## Integration Test

- 学習対象、出典、グループ、タグをまとめて登録できる
- 同一Sourceの `source_item_key` 重複時に409を返す
- 無効なタグコードで400を返す
- 学習記録の登録・更新・削除で現在状態が変わる
- アーカイブが物理削除ではなく `archived_at` の更新になる
- LLM Clientをモックし、構造化出力が候補として保存される

## Frontend Test

- 一覧の絞り込み
- 登録フォームの必須入力と難易度方式の切替
- 固定タグの選択
- LLM候補の採用・修正・却下

---

# 13. Error Handling

| ケース | 扱い |
| --- | --- |
| LearningItem不存在 | 404 |
| StudyRecord不存在 | 404 |
| 不正入力 | 400 |
| 出典内識別子の重複 | 409 |
| 無効なタグコード | 400 |
| LLM用の本文不足 | 400 |
| 対応プロンプトなし | 400 |
| LLMの構造化出力が不正 | 502 |
| DB障害 | 500 |

C#内部では、少なくとも `NotFoundException`、`DuplicateReferenceException`、`InvalidTagCodeException`、`InvalidAnalysisOutputException` を定義し、API LayerでProblem Detailsへ変換する。

---

# 14. Decision Log

## D-001 SQLiteとEntity Framework Coreを採用

### Decision

SQLiteとEntity Framework Coreを使用する。

### Reason

MVPは個人利用であり、C#とEF Coreの学習目的に合うため。

## D-002 LearningItemを中心にする

### Decision

`Problem` ではなく `LearningItem` を主要ドメインとする。

### Reason

競技プログラミング問題だけでなく、英単語、英文、教材なども同じ仕組みで扱うため。

## D-003 出典情報を分離する

### Decision

URLと出典内の識別子は `item_references` に持たせ、`learning_items` には持たせない。

### Reason

手入力の対象には外部識別子が不要であり、1つの対象が複数の出典を持つこともあるため。

## D-004 タグは固定カタログにする

### Decision

TagCodeをC# enumで定義し、`tags` テーブルにはシードデータとして保存する。

### Reason

分類の揺れを防ぎ、LLMが返せるタグを制限できるため。

## D-005 難易度は手動またはLLM候補で設定する

### Decision

ユーザーは難易度を手動で確定するか、LLM候補を確認して確定するかを選べる。

### Reason

手動で決めたい対象と、分析を補助に使いたい対象の両方を扱うため。

## D-006 プロンプトはコードで管理する

### Decision

プロンプト本文はDBに保存せず、アプリケーション設定として管理する。

### Reason

レビューとバージョン管理をコード変更として行い、DBには結果の再現に必要なキーと版だけを保存するため。

## D-007 LLMの生レスポンスと自己申告の自信度は保存しない

### Decision

`raw_response`、`confidence`、`reasoning` はMVPのテーブルに含めない。

### Reason

構造化出力を検証して必要な値だけを保存すればアプリ機能には十分であり、LLMの自己申告の自信度には明確な算出根拠がないため。

---

# 15. Non-Goals / Constraints

- MVPでは認証と複数ユーザー対応を実装しない
- 任意のユーザー定義属性・ユーザー定義タグを実装しない
- LLMの結果をユーザー確認なしで確定しない
- すべての出典に専用プロンプトを必須にしない
- 過剰なClean Architectureや不要な外部ライブラリを導入しない
