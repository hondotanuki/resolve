# API契約の棚卸し

> この文書はIssue #29時点の契約の記録です。Issue #32で問題区分と学習記録のAPI契約を変更中のため、現在のAPIとは一致しません。最新の契約はDevelopment環境のOpenAPIを参照してください。

## 目的・見方

実装済みのLearningItem・StudyRecord APIについて、APIの挙動、OpenAPI、OrvalのFrontend生成型をまとめる。プロダクト全体の仕様は [PROJECT_DESIGN.md](PROJECT_DESIGN.md)、OpenAPI・Scalarの利用方法は [README.md](../README.md) を参照する。

- 対象は実装済みの7エンドポイント。未実装のAPIは含めない。
- 「必須」「省略可」はJSONの項目の有無を、「null可」は値として `null` を受け付けることを表す。
- TypeScriptの `?` は項目を省略できることを表す。型名はOrvalが生成したモデルを指す。
- 実装、APIテスト、OpenAPI、生成型を照合しているが、すべての入力を網羅してテストしたものではない。

## 共通事項と表現上の制約

- `itemType` は `algorithm_problem`・`english_word`・`english_article`、`result` は `success`・`failure`・`reviewed`、`status` は `unstarted`・`needs_review`・`completed`。OpenAPIの `enum` と生成型に反映される。入力時はAPI側でも検証する。
- 空文字や空白のみのタイトルは拒否する。OpenAPIには最小長1と説明を記載し、空白のみの判定はAPI側で行う。
- 学習日時は `Z` 付きUTCで指定し、日時の初期値は拒否する。OpenAPIには `date-time` と説明を記載する。生成型の `string` だけではUTCや初期値を検証できない。
- 所要時間は0以上の `int32` または `null`。OpenAPIに数値範囲を記載する。TypeScriptの `number` は小数や範囲外の値も表すため、API側でも検証する。
- パスの `id`・`learningItemId`・`studyRecordId` は必須のルート値。OpenAPIは `integer` / `int32`、生成型は `number`。`:int` ルート制約に一致しない値はエンドポイントに到達しない。
- JSONの数値読み取りは `JsonNumberHandling.Strict`。数値文字列は数値として受け付けない。
- APIとOpenAPIはJSONの項目名の大文字・小文字を区別し、camelCaseを使う。APIは未知の項目を無視する。
- `Results.BadRequest()`・`Results.NotFound()` を直接返す場合はbodyなし。JSON読取などフレームワーク側の失敗は経路・環境によって異なり得るため、すべての400がbodyなしとは断定しない。OpenAPI上の400・404・204にはResponse schemaがなく、生成クライアントでは `data` は `void` として扱う。

根拠：[Program.cs](../backend/src/Resolve.Api/Program.cs)、[APIの値](../backend/src/Resolve.Api/Contracts/ApiValues.cs)、[入力制約テスト](../backend/tests/Resolve.Api.Tests/InputConstraintsApiTests.cs)、[生成クライアント](../frontend/src/api/generated/resolve-api/resolve-api.ts)。

## LearningItem

### POST /learning-items

#### Request

| 項目 | API / OpenAPI | Frontend生成型 |
| --- | --- | --- |
| `itemType` | 必須。定義済みの種別 | `CreateLearningItemRequestItemType` |
| `title` | 必須。空文字・空白のみは不可 | `string` |
| `content` | 省略可・null可の文字列 | `content?: string \| null` |

#### Response

201のbodyは登録した対象のオブジェクト。`id`・`itemType`・`title`・`archivedAt` を必ず返す。`archivedAt` はnull可で、生成型も `string | null`。

- `201`：登録成功。JSON bodyとLocationヘッダーを返す。
- `400`：未定義の種別、空・空白のみのタイトルなど。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemEndpoint.cs)、[Request](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemRequest.cs)、[Response](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/CreateLearningItemApiTests.cs)。

### GET /learning-items

body・パスパラメーターなし。200のbodyは配列で、対象がなければ `[]` を返す。通常一覧にアーカイブ済みの対象は含めない。

各要素の `id`・`itemType`・`title`・`archivedAt` は必須。`itemType` は定義済みの種別で、`archivedAt` はnull可。生成型も `archivedAt: string | null`。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/List/ListLearningItemsEndpoint.cs)、[Response](../backend/src/Resolve.Api/LearningItems/List/ListLearningItemsResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/ListLearningItemsApiTests.cs)。

### GET /learning-items/{id}

パスの `id` が必要。bodyなし。

200のbodyには `id`・`itemType`・`title`・`status`・`content`・`archivedAt`・`createdAt`・`updatedAt` を必ず返す。`content`・`archivedAt` はnull可で、生成型もそれぞれ `string | null`。`itemType`・`status` は定義済みの選択肢を表す生成型になる。

- `200`：JSONオブジェクト。
- `404`：対象なし。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/GetById/GetLearningItemByIdEndpoint.cs)、[Response](../backend/src/Resolve.Api/LearningItems/GetById/GetLearningItemResponse.cs)。

## StudyRecord

### POST /learning-items/{learningItemId}/study-records

パスの `learningItemId` が必要。

#### Request

| 項目 | API / OpenAPI | Frontend生成型 |
| --- | --- | --- |
| `result` | 必須。定義済みの学習結果 | `CreateStudyRecordRequestResult` |
| `studiedAt` | 必須。`Z` 付きUTC、初期値以外 | `string` |
| `durationSeconds` | 省略可・null可。0以上の `int32` | `durationSeconds?: number \| null` |
| `memo` | 省略可・null可の文字列 | `memo?: string \| null` |

#### Response

201のbodyには `id`・`learningItemId`・`result`・`studiedAt`・`durationSeconds`・`memo`・`createdAt`・`updatedAt`・`status` を必ず返す。`durationSeconds`・`memo` はnull可で、生成型もそれぞれ `number | null`・`string | null`。`result`・`status` は定義済みの選択肢を表す生成型になる。

- `201`：登録成功。JSON bodyとLocationヘッダーを返す。
- `400`：不正な結果・日時・所要時間など。
- `404`：LearningItemが存在しない。

Locationの `/study-records/{id}` はPATCH・DELETEに利用できるが、単体GETは未実装。単体GETは「feat: 復習対象を推薦する」で扱う方針とする。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordEndpoint.cs)、[Request](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordRequest.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/CreateStudyRecordApiTests.cs)。

### GET /learning-items/{learningItemId}/study-records

パスの `learningItemId` が必要。bodyなし。200のbodyは履歴の配列で、履歴がなければ `[]` を返す。学習日時の降順、同日時はID降順。

各要素の `id`・`learningItemId`・`result`・`studiedAt`・`durationSeconds`・`memo` は必須。`durationSeconds`・`memo` はnull可で、生成型もそれぞれ `number | null`・`string | null`。`result` は定義済みの選択肢を表す生成型になる。

- `200`：JSON配列。履歴なしの場合も200。
- `404`：LearningItemが存在しない。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/List/ListStudyRecordsEndpoint.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/List/ListStudyRecordsResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/ListStudyRecordsApiTests.cs)。

### PATCH /study-records/{studyRecordId}

パスの `studyRecordId` とbodyが必要。以下の既知の更新項目を1つ以上指定する。空の `{}` や未知の項目だけの場合は400。既知の項目と混在する未知の項目は無視する。

| 項目 | 省略時 | `null` | API / OpenAPI | Frontend生成型のプロパティ |
| --- | --- | --- | --- | --- |
| `result` | 変更しない | 拒否 | 定義済みの学習結果 | `result?: UpdateStudyRecordRequestResult` |
| `studiedAt` | 変更しない | 拒否 | `Z` 付きUTC、初期値以外 | `studiedAt?: string` |
| `durationSeconds` | 変更しない | 消去 | 0以上の `int32` | `durationSeconds?: number \| null` |
| `memo` | 変更しない | 消去 | 文字列 | `memo?: string \| null` |

OpenAPIの `anyOf` は1項目以上を要求するが、Orvalの生成型だけでは空のオブジェクトを防げない。Frontendでは [更新用ラッパー](../frontend/src/api/studyRecordUpdates.ts) と型ガードで補う。生成Hookの直接利用にはこの型制約が適用されない。

200のbodyには `id`・`learningItemId`・`result`・`studiedAt`・`durationSeconds`・`memo`・`createdAt`・`updatedAt` を必ず返す。`durationSeconds`・`memo` はnull可で、生成型もそれぞれ `number | null`・`string | null`。

- `200`：更新後のJSONオブジェクト。
- `400`：空の更新、不正な値など。
- `404`：記録が存在しない。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordEndpoint.cs)、[Request](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordRequest.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/UpdateStudyRecordApiTests.cs)。

### DELETE /study-records/{studyRecordId}

パスの `studyRecordId` が必要。bodyなし。

- `204`：削除成功。bodyなし。
- `404`：記録が存在しない。bodyなし。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Delete/DeleteStudyRecordEndpoint.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/DeleteStudyRecordApiTests.cs)。
