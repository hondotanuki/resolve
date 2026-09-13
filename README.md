# Resolve

学習対象・学習記録を管理し、復習を支援するアプリケーションです。C# / ASP.NET Core / React を学びながら開発しています。

## 必要な環境

- .NET 10 SDK

## バックエンドの起動

```bash
dotnet run --project backend/src/Resolve.Api --launch-profile http
```

## 品質確認

```bash
# 初回または clone 後
dotnet tool restore

# C# の書式確認・自動整形
dotnet csharpier check backend
dotnet csharpier format backend

# ビルド
dotnet build backend/Resolve.slnx
```
