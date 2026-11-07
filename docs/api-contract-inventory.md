# API契約の棚卸し

## 目的・見方

Issue #29で、実装済みのLearningItem・StudyRecord APIについて、実際の挙動、生成されたOpenAPI、OrvalのFrontend型を照合する。これは調査時点の記録であり、今後の契約変更を決定するものではない。プロダクト仕様は [PROJECT_DESIGN.md](PROJECT_DESIGN.md) を参照する。

- 調査対象：現在の7エンドポイント。未実装のAPIは含めない。
- 確認方法：API・Domainの実装と既存テスト、開発環境の `/openapi/v1.json`（OpenAPI 3.1.1）、生成済みTypeScriptを照合。OpenAPIは2026-11-07に取得。
- 表の「必須」「省略可」はJSONの項目の有無を、「null可」は値として `null` を受け付けることを表す。Responseの省略可はOpenAPI上の定義であり、実際に省略されることを意味しない。
- 実際の挙動は、実装またはテストから確認した範囲を記載する。網羅的な入力テストを実施したわけではない。
- 以下のTypeScriptの `?` は項目の省略可能性を表す。OpenAPIの `date-time` や `int32` はTypeScriptの `string` / `number` だけでは表せない。

## 食い違いと相談事項

1. **固定の選択肢**：`itemType`・`result`・`status` は実装で値を限定しているが、OpenAPIと生成型は `string`。どの型・変換方法で既存のJSON表現を維持しつつ選択肢を示すか検討する。
2. **タイトル**：空文字・空白のみをDomainで拒否するが、OpenAPIの `string` にその制約はない。
3. **日時**：StudyRecordの学習日時は `DateTime.Kind` がUTCである必要がある。OpenAPIの `date-time` だけではUTC限定を示さず、生成型も `string`。`Z` 付きはテストで確認済み、`+00:00` を含むオフセット付き・タイムゾーンなしは現状のAPIテストで拒否される。
4. **所要時間**：APIは0以上の `int32` または `null` を受け付ける。OpenAPIは `integer` / `int32`・null可だが、0以上の指定はない。TypeScriptの `number` は小数も表せる。
5. **nullを返すResponse項目**：OpenAPI・Frontendの型は `archivedAt`・`content`・`durationSeconds`・`memo` を省略可かつnull可としている。現在のJSON設定ではnullを含むプロパティを省略する設定はなく、登録・一覧等のテストでも一部の項目は `null` として返る。**決定事項**：値がない場合も項目を省略せず、`null` を返す契約に揃える。
6. **空のPATCH**：APIは `{}` を400で拒否するが、OpenAPIには少なくとも1項目必要という制約がない。生成型も `{}` を表せる。
7. **レスポンスのLocation**：StudyRecord登録は `/study-records/{id}` をLocationに設定する。このURIはPATCH・DELETEで使用できるが、単体取得GETは現在存在しない。**決定事項**：Issue #29ではLocationを変更せず、単体GETの追加は「feat: 復習対象を推薦する」のIssueで扱う。

## 共通事項

- パスの `id`・`learningItemId`・`studyRecordId` は必須のルート値。OpenAPIは `integer` / `int32`、生成クライアントは `number`。`:int` ルート制約に一致しない値はエンドポイントに到達しない。
- JSONの数値読み取りは `JsonNumberHandling.Strict`。文字列の数値を数値として受け付けない設定はAPI全体に適用される。
- 実装が `Results.BadRequest()`・`Results.NotFound()` を直接返す場合はbodyなし。JSON読取などのフレームワーク側の失敗は経路・環境によって応答が異なり得るため、すべての400がbodyなしとは断定しない。
- OpenAPI上、400・404・204にResponse schemaの定義はない。生成クライアントではこれらの `data` は `void` として扱う。

根拠：[Program.cs](../backend/src/Resolve.Api/Program.cs)、[生成クライアント](../frontend/src/api/generated/resolve-api/resolve-api.ts)、[API一覧](../backend/src/Resolve.Api/LearningItems/LearningItemEndpoints.cs)、[StudyRecord API一覧](../backend/src/Resolve.Api/StudyRecords/StudyRecordEndpoints.cs)。

## LearningItem

### POST /learning-items

#### Request

| 項目       | APIの実際の挙動                                                   | OpenAPI                 | Frontendの生成型           | 判定・補足                 |
| ---------- | ----------------------------------------------------------------- | ----------------------- | -------------------------- | -------------------------- |
| `itemType` | 必須。`algorithm_problem`・`english_word`・`english_article` のみ | 必須 `string`           | `itemType: string`         | 許可値が未反映             |
| `title`    | 必須。空文字・空白のみは400                                       | 必須 `string`           | `title: string`            | 空白を拒否する制約が未反映 |
| `content`  | 省略可・null可の文字列                                            | 省略可・null可 `string` | `content?: string \| null` | 一致                       |

#### Response

201のbodyはオブジェクト。

| 項目         | APIの実際の挙動                          | OpenAPI                               | Frontendの生成型              | 判定・補足                           |
| ------------ | ---------------------------------------- | ------------------------------------- | ----------------------------- | ------------------------------------ |
| `id`         | 登録したIDを返す                         | 必須 `integer` / `int32`              | `id: number`                  | 数値型の表現差                       |
| `itemType`   | 上記3値のいずれか                        | 必須 `string`                         | `itemType: string`            | 許可値が未反映                       |
| `title`      | 登録したタイトル                         | 必須 `string`                         | `title: string`               | 制約はRequest参照                    |
| `archivedAt` | 登録時は `null` を返すことをテストで確認 | 省略可・null可 `string` / `date-time` | `archivedAt?: string \| null` | 項目の省略可能性が実際より広い可能性 |

#### ステータスコード

- `201`：登録成功。JSON bodyとLocationヘッダーを返す。
- `400`：未定義の種別、空・空白のみのタイトルなど。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemEndpoint.cs)、[Request](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemRequest.cs)、[Response](../backend/src/Resolve.Api/LearningItems/Create/CreateLearningItemResponse.cs)、[Domain](../backend/src/Resolve.Domain/LearningItem.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/CreateLearningItemApiTests.cs)。

### GET /learning-items

#### Request

body・パスパラメーターなし。

#### Response

200のbodyは配列。該当なしなら `[]`。通常一覧にはアーカイブ済みを含めない。

| 項目（配列の要素） | APIの実際の挙動                              | OpenAPI                               | Frontendの生成型              | 判定・補足             |
| ------------------ | -------------------------------------------- | ------------------------------------- | ----------------------------- | ---------------------- |
| `id`               | 項目のID                                     | 必須 `integer` / `int32`              | `id: number`                  | 数値型の表現差         |
| `itemType`         | 定義済みの種別                               | 必須 `string`                         | `itemType: string`            | 許可値が未反映         |
| `title`            | タイトル                                     | 必須 `string`                         | `title: string`               | —                      |
| `archivedAt`       | 通常一覧で `null` を返すケースをテストで確認 | 省略可・null可 `string` / `date-time` | `archivedAt?: string \| null` | 項目の省略可能性を検討 |

#### ステータスコード

- `200`：JSON配列。対象がない場合も200。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/List/ListLearningItemsEndpoint.cs)、[Response](../backend/src/Resolve.Api/LearningItems/List/ListLearningItemsResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/ListLearningItemsApiTests.cs)。

### GET /learning-items/{id}

#### Request

パスの `id` が必要。bodyなし。

#### Response

200のbodyはオブジェクト。

| 項目         | APIの実際の挙動                          | OpenAPI                               | Frontendの生成型              | 判定・補足                        |
| ------------ | ---------------------------------------- | ------------------------------------- | ----------------------------- | --------------------------------- |
| `id`         | 対象のID                                 | 必須 `integer` / `int32`              | `id: number`                  | 数値型の表現差                    |
| `itemType`   | 定義済みの種別                           | 必須 `string`                         | `itemType: string`            | 許可値が未反映                    |
| `title`      | タイトル                                 | 必須 `string`                         | `title: string`               | —                                 |
| `status`     | `unstarted`・`needs_review`・`completed` | 必須 `string`                         | `status: string`              | 許可値が未反映                    |
| `content`    | 内容または `null`                        | 省略可・null可 `string`               | `content?: string \| null`    | 項目の省略可能性を検討            |
| `archivedAt` | 日時または `null`                        | 省略可・null可 `string` / `date-time` | `archivedAt?: string \| null` | 項目の省略可能性を検討            |
| `createdAt`  | 作成日時                                 | 必須 `string` / `date-time`           | `createdAt: string`           | UTCという意味は型だけでは表せない |
| `updatedAt`  | 更新日時                                 | 必須 `string` / `date-time`           | `updatedAt: string`           | 同上                              |

#### ステータスコード

- `200`：JSONオブジェクト。
- `404`：対象なし。

根拠：[Endpoint](../backend/src/Resolve.Api/LearningItems/GetById/GetLearningItemByIdEndpoint.cs)、[Response](../backend/src/Resolve.Api/LearningItems/GetById/GetLearningItemResponse.cs)、[状態変換](../backend/src/Resolve.Api/Mapping/ItemStatusMapping.cs)。

## StudyRecord

### POST /learning-items/{learningItemId}/study-records

#### Request

パスの `learningItemId` が必要。

| 項目              | APIの実際の挙動                                        | OpenAPI                            | Frontendの生成型                   | 判定・補足                                 |
| ----------------- | ------------------------------------------------------ | ---------------------------------- | ---------------------------------- | ------------------------------------------ |
| `result`          | 必須。`success`・`failure`・`reviewed` のみ            | 必須 `string`                      | `result: string`                   | 許可値が未反映                             |
| `studiedAt`       | 必須。読み取った `DateTime.Kind` がUTC、かつ初期値以外 | 必須 `string` / `date-time`        | `studiedAt: string`                | UTC限定・初期値拒否は未反映                |
| `durationSeconds` | 省略可・null可。0以上の `int32`                        | 省略可・null可 `integer` / `int32` | `durationSeconds?: number \| null` | 最小値が未反映。TSの `number` は小数も表す |
| `memo`            | 省略可・null可の文字列                                 | 省略可・null可 `string`            | `memo?: string \| null`            | 一致                                       |

#### Response

201のbodyはオブジェクト。

| 項目                     | APIの実際の挙動        | OpenAPI                               | Frontendの生成型                   | 判定・補足                    |
| ------------------------ | ---------------------- | ------------------------------------- | ---------------------------------- | ----------------------------- |
| `id`・`learningItemId`   | 登録した記録と対象のID | 各項目とも必須 `integer` / `int32`    | 各項目とも `number`                | 数値型の表現差                |
| `result`                 | 定義済みの結果         | 必須 `string`                         | `result: string`                   | 許可値が未反映                |
| `studiedAt`              | UTCの学習日時          | 必須 `string` / `date-time`           | `studiedAt: string`                | UTCの意味は型だけでは表せない |
| `durationSeconds`        | 所要時間または `null`  | 省略可・null可 `integer` / `int32`    | `durationSeconds?: number \| null` | 項目の省略可能性を検討        |
| `memo`                   | メモまたは `null`      | 省略可・null可 `string`               | `memo?: string \| null`            | 項目の省略可能性を検討        |
| `createdAt`・`updatedAt` | 作成日時・更新日時     | 各項目とも必須 `string` / `date-time` | 各項目とも `string`                | UTCの意味は型だけでは表せない |
| `status`                 | 登録後の現在状態       | 必須 `string`                         | `status: string`                   | 許可値が未反映                |

#### ステータスコード

- `201`：登録成功。JSON bodyとLocationヘッダーを返す。
- `400`：不正な結果・日時・所要時間など。
- `404`：LearningItemが存在しない。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordEndpoint.cs)、[Request](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordRequest.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/Create/CreateStudyRecordResponse.cs)、[Domain](../backend/src/Resolve.Domain/StudyRecord.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/CreateStudyRecordApiTests.cs)。

### GET /learning-items/{learningItemId}/study-records

#### Request

パスの `learningItemId` が必要。bodyなし。

#### Response

200のbodyは配列。履歴がなければ `[]`。学習日時の降順、同日時はID降順。

| 項目（配列の要素）     | APIの実際の挙動       | OpenAPI                            | Frontendの生成型                   | 判定・補足                    |
| ---------------------- | --------------------- | ---------------------------------- | ---------------------------------- | ----------------------------- |
| `id`・`learningItemId` | 記録と対象のID        | 各項目とも必須 `integer` / `int32` | 各項目とも `number`                | 数値型の表現差                |
| `result`               | 定義済みの結果        | 必須 `string`                      | `result: string`                   | 許可値が未反映                |
| `studiedAt`            | UTCの学習日時         | 必須 `string` / `date-time`        | `studiedAt: string`                | UTCの意味は型だけでは表せない |
| `durationSeconds`      | 所要時間または `null` | 省略可・null可 `integer` / `int32` | `durationSeconds?: number \| null` | 項目の省略可能性を検討        |
| `memo`                 | メモまたは `null`     | 省略可・null可 `string`            | `memo?: string \| null`            | 項目の省略可能性を検討        |

#### ステータスコード

- `200`：JSON配列。LearningItemが存在し履歴がない場合も200。
- `404`：LearningItemが存在しない。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/List/ListStudyRecordsEndpoint.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/List/ListStudyRecordsResponse.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/ListStudyRecordsApiTests.cs)。

### PATCH /study-records/{studyRecordId}

#### Request

パスの `studyRecordId` が必要。bodyは必須で、空の `{}` は400。

| 項目              | 省略時     | `null` | APIが受け付ける値 | OpenAPI / Frontend                                                        | 判定・補足          |
| ----------------- | ---------- | ------ | ----------------- | ------------------------------------------------------------------------- | ------------------- |
| `result`          | 変更しない | 拒否   | 3種類の結果       | 省略可の `string` / `result?: string`                                     | 許可値が未反映      |
| `studiedAt`       | 変更しない | 拒否   | UTCの学習日時     | 省略可の `string` / `date-time` / `studiedAt?: string`                    | UTC限定は未反映     |
| `durationSeconds` | 変更しない | 消去   | 0以上の `int32`   | 省略可・null可の `integer` / `int32` / `durationSeconds?: number \| null` | 0以上の指定が未反映 |
| `memo`            | 変更しない | 消去   | 文字列            | 省略可・null可の `string` / `memo?: string \| null`                       | 一致                |

#### Response

200のbodyは更新後のオブジェクト。

| 項目                     | APIの実際の挙動       | OpenAPI                               | Frontendの生成型                   | 判定・補足                    |
| ------------------------ | --------------------- | ------------------------------------- | ---------------------------------- | ----------------------------- |
| `id`・`learningItemId`   | 記録と対象のID        | 各項目とも必須 `integer` / `int32`    | 各項目とも `number`                | 数値型の表現差                |
| `result`                 | 定義済みの結果        | 必須 `string`                         | `result: string`                   | 許可値が未反映                |
| `studiedAt`              | UTCの学習日時         | 必須 `string` / `date-time`           | `studiedAt: string`                | UTCの意味は型だけでは表せない |
| `durationSeconds`        | 所要時間または `null` | 省略可・null可 `integer` / `int32`    | `durationSeconds?: number \| null` | 項目の省略可能性を検討        |
| `memo`                   | メモまたは `null`     | 省略可・null可 `string`               | `memo?: string \| null`            | 項目の省略可能性を検討        |
| `createdAt`・`updatedAt` | 作成日時・更新日時    | 各項目とも必須 `string` / `date-time` | 各項目とも `string`                | UTCの意味は型だけでは表せない |

#### ステータスコード

- `200`：更新後のJSONオブジェクト。
- `400`：空の更新、不正な値など。
- `404`：記録が存在しない。

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordEndpoint.cs)、[Request](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordRequest.cs)、[Response](../backend/src/Resolve.Api/StudyRecords/Update/UpdateStudyRecordResponse.cs)、[APIテスト・OpenAPIテスト](../backend/tests/Resolve.Api.Tests/UpdateStudyRecordApiTests.cs)。

### DELETE /study-records/{studyRecordId}

#### Request

パスの `studyRecordId` が必要。bodyなし。

#### Response・ステータスコード

| ステータス | APIの実際の挙動            | OpenAPI / Frontend        | 判定・補足 |
| ---------- | -------------------------- | ------------------------- | ---------- |
| `204`      | 削除成功。bodyなし         | schemaなし / `data: void` | 一致       |
| `404`      | 記録が存在しない。bodyなし | schemaなし / `data: void` | 一致       |

根拠：[Endpoint](../backend/src/Resolve.Api/StudyRecords/Delete/DeleteStudyRecordEndpoint.cs)、[APIテスト](../backend/tests/Resolve.Api.Tests/DeleteStudyRecordApiTests.cs)。
