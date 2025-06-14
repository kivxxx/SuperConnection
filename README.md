# SuperConnection

SuperConnection 是一個高效能的 SQL Server 資料庫連線管理與資料存取元件。

## 快速開始

```csharp
using Kiv.SuperConnection;

// 建立連線字串
string connectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;";

// 建立資料存取物件（使用連線池）
using var dataAccess = new SuperConnectionAccess(connectionString);

// 查詢資料
var dataTable = await dataAccess.ExecuteQueryAsync(
    "SELECT * FROM Users WHERE Age >= @Age",
    new Dictionary<string, object> { { "@Age", 18 } }
);

// 執行非查詢命令
var affectedRows = await dataAccess.ExecuteNonQueryAsync(
    "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
    new Dictionary<string, object>
    {
        { "@Name", "測試使用者" },
        { "@Age", 25 }
    }
);

// 查詢單一值
var count = await dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");

// 執行交易
await dataAccess.ExecuteTransactionAsync(async (transaction) =>
{
    await dataAccess.ExecuteNonQueryAsync(
        "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
        new Dictionary<string, object>
        {
            { "@Name", "測試使用者" },
            { "@Age", 25 }
        }
    );
});
```

## 連線模式

### 1. 連線池模式（預設）

```csharp
// 建立資料存取物件（使用連線池）
using var dataAccess = new SuperConnectionAccess(connectionString);

// 或指定連線池大小
using var dataAccess = new SuperConnectionAccess(
    connectionString,
    maxPoolSize: 100,  // 最大連線數
    minPoolSize: 5     // 最小連線數
);
```

特點：
- 自動管理連線池
- 適合需要頻繁查詢的場景
- 減少連線建立和斷開的開銷
- 支援交易操作
- 需要實作 IDisposable 介面

### 2. 即時斷開模式

```csharp
// 建立資料存取物件（不使用連線池）
var dataAccess = new SuperConnectionAccess(connectionString, useConnectionPool: false);
```

特點：
- 每次查詢時建立新連線
- 查詢完成後立即斷開連線
- 適合不需要頻繁查詢的場景
- 減少資料庫連線的佔用時間
- 不需要實作 IDisposable 介面
- 不支援交易操作

## 系統需求

- .NET 6.0 或更高版本
- SQL Server 2012 或更高版本
- Microsoft.Data.SqlClient 5.1.4 或更高版本

## 注意事項

1. 請確保正確設定連線字串
2. 建議使用參數化查詢以避免 SQL 注入
3. 適當選擇連線模式以符合使用場景
4. 使用連線池模式時記得正確釋放資源（使用 using 語句）
5. 交易操作必須使用連線池模式 
