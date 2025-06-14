# SuperConnection

SuperConnection 是一個高效能的 SQL Server 資料庫連線管理與資料存取元件，提供連線池管理、非同步操作支援和交易處理功能。

## 核心功能

- 自動連線池管理
- 非同步操作支援
- 交易處理
- 資源自動釋放
- 執行緒安全
- 高效能設計
- 支援參數化查詢
- 自動重連機制
- 連線狀態監控

## 系統需求

- .NET 6.0 或更高版本
- SQL Server 2012 或更高版本
- Microsoft.Data.SqlClient 5.1.4 或更高版本

## 安裝方式

### NuGet 套件

```bash
dotnet add package SuperConnection
```

### 手動安裝

1. 下載專案
2. 編譯專案
3. 將編譯後的 DLL 加入您的專案參考

## 基本使用

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

// 執行交易
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

## 最佳實踐

1. 連線管理
   - 使用 `using` 語句確保資源正確釋放
   - 適當設定連線池大小
   - 定期檢查連線狀態

2. 查詢優化
   - 使用參數化查詢
   - 避免使用 `SELECT *`
   - 適當使用索引
   - 使用適當的資料類型

3. 交易處理
   - 使用交易處理相關操作
   - 設定適當的交易隔離等級
   - 及時提交或回滾交易

## 授權

Copyright (c) 2024 Kiv. All rights reserved. 