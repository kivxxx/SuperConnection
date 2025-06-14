# SuperConnection

SuperConnection 是一個高效能的 SQL Server 資料庫連線管理與資料存取元件，提供連線池管理、非同步操作支援和交易處理功能。

## 功能特點

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

## 使用範例

### 基本使用

```csharp
using Kiv.SuperConnection;
using Microsoft.Data.SqlClient;

// 建立連線字串
string connectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;";

// 建立資料存取物件
using var dataAccess = new SuperConnectionAccess(connectionString);

// 查詢資料
var users = await dataAccess.GetListAsync<User>(
    "SELECT * FROM Users",
    reader => new User
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Age = reader.GetInt32(2)
    }
);
```

### 執行交易

```csharp
await dataAccess.ExecuteTransactionAsync(async (connection) =>
{
    // 插入資料
    await dataAccess.ExecuteNonQueryAsync(
        "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
        new SqlParameter("@Name", "測試使用者"),
        new SqlParameter("@Age", 25)
    );

    // 更新資料
    await dataAccess.ExecuteNonQueryAsync(
        "UPDATE Users SET Age = @Age WHERE Name = @Name",
        new SqlParameter("@Name", "測試使用者"),
        new SqlParameter("@Age", 26)
    );
});
```

### 查詢單一值

```csharp
// 查詢數量
var count = await dataAccess.GetScalarAsync<int>("SELECT COUNT(*) FROM Users");

// 查詢字串
var name = await dataAccess.GetScalarAsync<string>("SELECT Name FROM Users WHERE Id = @Id",
    new SqlParameter("@Id", 1));

// 查詢日期
var createDate = await dataAccess.GetScalarAsync<DateTime>("SELECT CreateDate FROM Users WHERE Id = @Id",
    new SqlParameter("@Id", 1));
```

### 查詢資料表

```csharp
// 基本查詢
var dataTable = await dataAccess.GetDataTableAsync(
    "SELECT * FROM Users WHERE Age >= @Age",
    new SqlParameter("@Age", 18)
);

// 使用 DataTable 處理資料
foreach (DataRow row in dataTable.Rows)
{
    Console.WriteLine($"使用者: {row["Name"]}, 年齡: {row["Age"]}");
}
```

### 批次處理

```csharp
// 批次插入資料
var parameters = new List<SqlParameter[]>();
for (int i = 0; i < 1000; i++)
{
    parameters.Add(new[]
    {
        new SqlParameter("@Name", $"User{i}"),
        new SqlParameter("@Age", 20 + i)
    });
}

await dataAccess.ExecuteTransactionAsync(async (connection) =>
{
    foreach (var param in parameters)
    {
        await dataAccess.ExecuteNonQueryAsync(
            "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
            param
        );
    }
});
```

## 進階設定

### 自訂連線池大小

```csharp
// 設定最大連線數為 200，最小連線數為 10
using var dataAccess = new SuperConnectionAccess(
    connectionString,
    maxPoolSize: 200,
    minPoolSize: 10
);
```

### 連線字串範例

```csharp
// 基本連線字串
string basicConnection = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;";

// 使用 Windows 驗證
string windowsAuthConnection = "Server=localhost;Database=TestDB;Trusted_Connection=True;";

// 設定連線超時
string timeoutConnection = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;Connection Timeout=30;";

// 設定命令超時
string commandTimeoutConnection = "Server=localhost;Database=TestDB;User Id=sa;Password=YourPassword;Command Timeout=30;";
```

## 錯誤處理

```csharp
try
{
    using var dataAccess = new SuperConnectionAccess(connectionString);
    
    // 執行資料庫操作
    var result = await dataAccess.GetListAsync<User>(...);
}
catch (SqlException ex)
{
    // 處理資料庫錯誤
    switch (ex.Number)
    {
        case -1: // 連線錯誤
            Console.WriteLine("資料庫連線失敗");
            break;
        case -2: // 超時錯誤
            Console.WriteLine("資料庫操作超時");
            break;
        case 208: // 物件不存在
            Console.WriteLine("查詢的物件不存在");
            break;
        case 2627: // 主鍵衝突
            Console.WriteLine("資料重複");
            break;
        default:
            Console.WriteLine($"資料庫錯誤: {ex.Message}");
            break;
    }
}
catch (Exception ex)
{
    // 處理其他錯誤
    Console.WriteLine($"發生錯誤: {ex.Message}");
}
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

4. 錯誤處理
   - 實作完整的錯誤處理機制
   - 記錄錯誤日誌
   - 實作重試機制

## 效能建議

1. 連線池設定
   - 根據系統負載調整連線池大小
   - 監控連線池使用情況
   - 定期清理無效連線

2. 查詢優化
   - 使用參數化查詢避免 SQL 注入
   - 使用適當的索引
   - 避免大量資料傳輸

3. 資源管理
   - 及時釋放不需要的連線
   - 使用交易處理大量資料操作
   - 實作適當的快取機制

## 常見問題

1. 連線問題
   - Q: 連線池已滿怎麼辦？
   - A: 調整連線池大小或檢查是否有連線未釋放

2. 效能問題
   - Q: 查詢執行緩慢怎麼辦？
   - A: 檢查索引、SQL 語句和連線池設定

3. 記憶體問題
   - Q: 記憶體使用過高怎麼辦？
   - A: 檢查是否有連線未釋放，調整連線池大小

## 授權條款

著作權所有 © 2025 Kiv

## 支援

如有問題或建議，請提交 Issue 或 Pull Request。

## 版本歷史

- 1.0.0
  - 初始版本
  - 基本連線池管理
  - 非同步操作支援
  - 交易處理功能
  - 參數化查詢支援
  - 自動重連機制
  - 連線狀態監控 