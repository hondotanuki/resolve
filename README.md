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

```bash
dotnet run --project backend/src/Resolve.Api --launch-profile http
```

Backendは次のURLで起動します。

- API: `http://localhost:5041`
- OpenAPI: `http://localhost:5041/openapi/v1.json`

## フロントエンドの起動

```bash
fnm use
cd frontend
npm install
npm run dev
```

Frontendは、開発時に`/learning-items`へのリクエストをBackendへ転送します。
BackendとFrontendは、それぞれ別のターミナルで起動してください。

## OpenAPIからAPIクライアントを生成

Backendを起動した状態で、Frontendから実行します。

```bash
cd frontend
npm run generate:api
```

生成されたクライアントは`frontend/src/api/generated/`に出力されます。
生成ファイルは手動で編集せず、BackendのOpenAPI定義または`frontend/orval.config.ts`を変更して再生成します。

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
