# Tailwind Traders Mail Service へようこそ

メールは好むと好まざるとにかかわらず、誰にでも必要です。このサービスは、API 経由でトランザクションメールを送信したり、MailChimp のようにタグやセグメントを使ってリストへの一括メール送信を行います。

> **注意:** このプロジェクトは現在活発に開発中です。コントリビューションやフィードバックを歓迎します！

## 含まれるもの

- **Server** — コンタクト管理、ブロードキャスト、メール送信のための .NET 8 Minimal API ([server/](./server/))
- **CLI** — Markdown ファイルからブロードキャストを作成する Node.js コマンドラインツール ([cli/](./cli/))
- **Jobs** — バックグラウンドメール処理と Azure 連携のための Go ベースのジョブランナー ([jobs/](./jobs/))
- **Database** — PostgreSQL スキーマとシードデータ ([db/](./db/))

## 前提条件

| ツール | バージョン | 備考 |
|--------|-----------|------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 以上 | サーバー API 用 |
| [PostgreSQL](https://www.postgresql.org/download/) | 14 以上 | メインデータベース |
| [Docker](https://docs.docker.com/get-docker/) | 最新版 | Mailpit およびコンテナ化ワークフロー用 |
| [Node.js](https://nodejs.org/) | LTS 20 以上 | CLI ツール用（任意） |
| [Go](https://go.dev/dl/) | 1.21 以上 | ジョブランナー用（任意） |

## クイックスタート

### 1. リポジトリをクローンする

```bash
git clone https://github.com/scubaninja/dotNET-mail-demo.git
cd dotNET-mail-demo
```

### 2. データベースをセットアップする

PostgreSQL が起動していることを確認し、データベースを作成してスキーマを読み込みます：

```bash
createdb tailwind
cd db
make db
```

サンプルデータのシードは任意です：

```bash
cd ../server
make seed
```

### 3. 環境変数を設定する

サーバー用の `.env` ファイルを作成（または変数をエクスポート）します。最低限以下が必要です：

```bash
ASPNETCORE_ENVIRONMENT="Development"
DATABASE_URL="postgres://localhost/tailwind"

# SMTP 設定 — Ethereal (https://ethereal.email) で無料のテスト用認証情報を取得できます
SMTP_USER=""
SMTP_PASSWORD=""
SMTP_HOST=""

DEFAULT_FROM="test@tailwind.dev"

# "local" に設定するとバックグラウンドメール送信ワーカーが有効になります
SEND_WORKER="local"
```

### 4. ローカルメールテストサーバーを起動する

[Mailpit](https://github.com/axllent/mailpit) は送信メールをキャプチャし、ブラウザで確認できるようにします：

```bash
cd server
make mailpit
```

[http://localhost:8025](http://localhost:8025) を開いてキャプチャされたメールを確認できます。

### 5. API サーバーを起動する

```bash
cd server
dotnet watch
```

API（Swagger UI 付き）は [http://localhost:5000](http://localhost:5000) で利用できます。

## API 概要

API は Swagger/OpenAPI で文書化されています。サーバー起動後、ルート URL にアクセスしてエンドポイントをインタラクティブに確認できます。

### パブリックエンドポイント

| メソッド | パス | 説明 |
|---------|------|------|
| `GET` | `/about` | API 情報 |
| `POST` | `/signup` | メーリングリストへの登録 |
| `GET` | `/unsubscribe/{key}` | 固有キーによる配信停止 |
| `GET` | `/link/clicked/{key}` | リンククリックの追跡 |

### 管理者エンドポイント

| 領域 | 説明 |
|------|------|
| Broadcasts | 一括メールキャンペーンの作成と管理 |
| Contacts | 購読者のコンタクトとタグの管理 |
| Bulk Operations | タグとセグメントの一括操作 |

## プロジェクト構成

```
dotNET-mail-demo/
├── server/               # .NET 8 Minimal API
│   ├── Api/              # ルートハンドラー（パブリック + 管理者）
│   ├── Models/           # データモデル（Contact, Message, Broadcast など）
│   ├── Data/             # データベースアクセス層（Dapper ORM）
│   ├── Services/         # バックグラウンド送信、AI、メール送信
│   ├── Commands/         # コマンドパターンの実装
│   ├── Tests/            # xUnit テスト
│   └── Makefile          # ビルド・実行・テストのショートカット
├── cli/                  # Markdown ベースのブロードキャスト用 Node.js CLI
├── db/                   # PostgreSQL スキーマ (db.sql) とシードデータ (seed.sql)
├── jobs/                 # Go ベースのジョブランナー（Mage）
├── deploy/               # デプロイメントリソース（Azure/Docker/K8s）
├── docs/                 # 追加ドキュメント
└── docker-compose.yml    # Docker Compose 設定
```

## テストの実行

テストは [xUnit](https://xunit.net/) を使用し、`server/Tests/` に配置されています。以下のコマンドで実行します：

```bash
cd server
make test
```

または直接実行：

```bash
cd server
dotnet test
```

## CLI の使い方

CLI は Markdown ファイルを読み込み、ブロードキャストを作成します。詳細は [cli/README.md](./cli/README.md) を参照してください。

```bash
cd cli
npm install
alias mdmail="node ./bin/mdmail.js"  # 永続化するにはシェルプロファイルに追加してください
```

## デプロイ

Azure、Docker、Kubernetes 用のデプロイメントリソースは [deploy/](./deploy/) ディレクトリにあります。[jobs/](./jobs/) サービスには Azure Container Apps、Service Bus などの Mage ターゲットが含まれています。詳細は [jobs/README.md](./jobs/README.md) を参照してください。

## コントリビューション

1. リポジトリをフォークする
2. フィーチャーブランチを作成する（`git checkout -b my-feature`）
3. 変更を加える
4. テストを実行する（`cd server && make test`）
5. プルリクエストを作成する

## ライセンス

このプロジェクトは [MIT License](https://opensource.org/license/mit/) の下でライセンスされています。