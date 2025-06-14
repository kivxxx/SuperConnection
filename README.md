# SuperConnection

高效、簡潔的 .NET 資料庫連線存取元件。

## 特色
- 支援連線池與即時斷開兩種連線管理模式
- 提供簡單的查詢、非查詢、交易操作 API
- 適用於 Microsoft SQL Server

## 安裝

請透過 NuGet 安裝：
```
dotnet add package Kiv.SuperConnection
```

## 快速開始

```csharp
using Kiv.SuperConnection;
using System.Data;

var connStr = "你的連線字串";
var dataAccess = new SuperConnectionAccess(connStr);
var dt = await dataAccess.ExecuteQueryAsync("SELECT * FROM account");
```

## 連線管理模式

- **連線池模式**（預設）：
  ```csharp
  var dataAccess = new SuperConnectionAccess(connStr);
  ```
- **即時斷開模式**：
  ```csharp
  var dataAccess = new SuperConnectionAccess(connStr, useConnectionPool: false);
  ```

## API 範例

- 查詢：
  ```csharp
  var dt = await dataAccess.ExecuteQueryAsync("SELECT * FROM account");
  ```
- 非查詢：
  ```csharp
  int rows = await dataAccess.ExecuteNonQueryAsync(
      "INSERT INTO account (account, password_hash) VALUES (@Account, @PasswordHash)",
      new Dictionary<string, object> { {"@Account", "user1"}, {"@PasswordHash", "hash1"} }
  );
  ```
- 交易：
  ```csharp
  await dataAccess.ExecuteTransactionAsync(async tran => {
      // 交易內多次操作
  });
  ```

## 注意事項
- 請依據你的資料表結構調整 SQL 與參數
- 連線字串請依實際環境設定
