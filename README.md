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

string connStr = "你的連線字串";

// 連線池模式（預設，建議使用 using 釋放資源）
using var dataAccess = new SuperConnectionAccess(connStr);

// 即時斷開模式（每次查詢自動開關連線）
var dataAccess2 = new SuperConnectionAccess(connStr, useConnectionPool: false);
```

## API 用法範例

### 1. 查詢（回傳 DataTable）
```csharp
var dt = await dataAccess.ExecuteQueryAsync("SELECT * FROM account");
```

### 2. 參數化查詢
```csharp
var dt = await dataAccess.ExecuteQueryAsync(
    "SELECT * FROM account WHERE account = @Account",
    new Dictionary<string, object> { {"@Account", "user1"} }
);
```

### 3. 非查詢（INSERT/UPDATE/DELETE）
```csharp
int rows = await dataAccess.ExecuteNonQueryAsync(
    "INSERT INTO account (account, password_hash) VALUES (@Account, @PasswordHash)",
    new Dictionary<string, object> { {"@Account", "user1"}, {"@PasswordHash", "hash1"} }
);
```

### 4. 查詢單一值（ExecuteScalar）
```csharp
int count = await dataAccess.ExecuteScalarAsync<int>(
    "SELECT COUNT(*) FROM account WHERE account = @Account",
    new Dictionary<string, object> { {"@Account", "user1"} }
);
```

### 5. 交易操作（多步驟）
```csharp
await dataAccess.ExecuteTransactionAsync(async tran =>
{
    await dataAccess.ExecuteNonQueryAsync(
        "UPDATE account SET full_name = @Name WHERE account = @Account",
        new Dictionary<string, object> { {"@Name", "新名字"}, {"@Account", "user1"} }
    );
    await dataAccess.ExecuteNonQueryAsync(
        "INSERT INTO log (msg, created_at) VALUES (@Msg, @Time)",
        new Dictionary<string, object> { {"@Msg", "更新帳號 user1"}, {"@Time", DateTime.Now} }
    );
});
```

## 連線管理模式

- **連線池模式**（預設）：
  ```csharp
  using var dataAccess = new SuperConnectionAccess(connStr);
  ```
- **即時斷開模式**：
  ```csharp
  var dataAccess = new SuperConnectionAccess(connStr, useConnectionPool: false);
  ```

## 注意事項
- 請依據你的資料表結構調整 SQL 與參數
- 連線字串請依實際環境設定
- 交易操作建議使用連線池模式
