# SuperConnection

SuperConnection 是一個高效能的 SQL Server 資料庫連線管理與資料存取元件，專為 .NET 應用程式設計。

## 功能列表

1. **連線管理**
   - 自動連線池管理
   - 自動重連機制
   - 連線狀態監控
   - 資源自動釋放

2. **資料操作**
   - 非同步查詢
   - 交易處理
   - 參數化查詢
   - 批次處理

## 系統需求

- .NET 6.0 或更高版本
- SQL Server 2012 或更高版本
- Microsoft.Data.SqlClient 5.1.4 或更高版本

## 使用方法

### 1. 基本查詢

```csharp
using Kiv.SuperConnection;
using Microsoft.Data.SqlClient;

// 建立連線字串
string connectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;";

// 建立資料存取物件
using var dataAccess = new SuperConnectionAccess(connectionString);

// 查詢資料
var dataTable = await dataAccess.ExecuteQueryAsync(
    "SELECT * FROM Users WHERE Age >= @Age",
    new Dictionary<string, object> { { "@Age", 18 } }
);
```

### 2. 執行交易

```csharp
await dataAccess.ExecuteTransactionAsync(async (transaction) =>
{
    // 插入資料
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

### 3. 查詢單一值

```csharp
// 查詢數量
var count = await dataAccess.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");

// 查詢字串
var name = await dataAccess.ExecuteScalarAsync<string>(
    "SELECT Name FROM Users WHERE Id = @Id",
    new Dictionary<string, object> { { "@Id", 1 } }
);
```

### 4. 執行非查詢命令

```csharp
// 插入資料
await dataAccess.ExecuteNonQueryAsync(
    "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
    new Dictionary<string, object>
    {
        { "@Name", "測試使用者" },
        { "@Age", 25 }
    }
);

// 更新資料
await dataAccess.ExecuteNonQueryAsync(
    "UPDATE Users SET Age = @Age WHERE Name = @Name",
    new Dictionary<string, object>
    {
        { "@Name", "測試使用者" },
        { "@Age", 26 }
    }
);
```

### 5. 自訂連線池大小

```csharp
using var dataAccess = new SuperConnectionAccess(
    connectionString,
    maxPoolSize: 200,  // 最大連線數
    minPoolSize: 10    // 最小連線數
);
```

## 授權

Copyright (c) 2024 Kiv. All rights reserved. 