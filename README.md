# Resolve

学習対象・学習記録を管理し、復習を支援するアプリケーションです。C# / ASP.NET Core / React を学びながら開発しています。

## 必要な環境

- .NET 10 SDK
- Node.js
- npm
- fnm（Node.js のバージョン管理に使用）

Node.jsと.NET SDKのバージョンは、リポジトリ直下の次のファイルで管理しています。

- `.node-version`
- `global.json`

## ディレクトリ構成

```text
backend/
  src/
    Resolve.Domain/
    Resolve.Application/
    Resolve.Infrastructure/
    Resolve.Api/
  tests/
frontend/
docs/
```

## バックエンドの起動

初回はローカルツールを復元し、DBを作成します。

```bash
dotnet tool restore
dotnet ef database update --project backend/src/Resolve.Infrastructure --startup-project backend/src/Resolve.Api
```

問題区分と学習記録の目標時間を追加するMigrationは、既存の問題・記録があるDBには適用できません。開発用DBを作り直す場合は、必要なデータを退避してから対象のSQLite DBを削除し、上のコマンドを実行してください。既定のDBは `backend/src/Resolve.Api/resolve.db` です。

問題URLとコンテスト情報を追加するMigrationは、既存データを残したまま適用できます。新しい項目は、URL登録機能を追加するまでは未設定です。既存DBへの適用にも上の `dotnet ef database update` を使用します。

```bash
dotnet run --project backend/src/Resolve.Api --launch-profile http
```

Backendは次のURLで起動します。

- API: `http://localhost:5041`
- OpenAPI: `http://localhost:5041/openapi/v1.json`
- Scalar: `http://localhost:5041/scalar/v1`

## フロントエンドの起動

問題は現在、タイトル・出典・コンテスト種別・問題区分を手入力して登録します。URLからの自動登録とタイマーは今後実装します。

```bash
fnm use
cd frontend
npm install
npm run dev
```

Frontendは、開発時に`/api/learning-items`と`/api/study-records`へのリクエストをBackendへ転送します。
BackendとFrontendは、それぞれ別のターミナルで起動してください。

## OpenAPIからAPIクライアントを生成

BackendをDevelopment環境で起動すると、上記URLでOpenAPIのJSONとScalarのAPI画面を確認できます。
Scalarではエンドポイント、入力、レスポンスを確認できます。

API契約を変更した場合は、Backendを再起動し、Frontendからクライアントを再生成します。

```bash
cd frontend
npm run generate:api
```

生成されたクライアントは`frontend/src/api/generated/`に出力されます。
生成ファイルは手動で編集せず、BackendのOpenAPI定義または`frontend/orval.config.ts`を変更して再生成します。
生成後は`npm run check`を実行し、Frontendとの整合性を確認します。
学習記録のPATCHには、空の更新を防ぐため`frontend/src/api/studyRecordUpdates.ts`のラッパーを使います。

## 品質確認

```bash
# 初回または clone 後
dotnet tool restore

# C# の書式確認・自動整形
dotnet csharpier check backend
dotnet csharpier format backend

# Backendのビルド・テスト
dotnet build backend/Resolve.slnx
dotnet test backend/Resolve.slnx

# Frontendの静的検査・整形確認・テスト・ビルド
cd frontend
npm run check
```

Frontendの個別コマンドは次のとおりです。

```bash
npm run lint
npm run format
npm run format:check
npm run test
npm run build
```
