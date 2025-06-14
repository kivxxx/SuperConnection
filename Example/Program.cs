/*
 * SuperConnection
 * Copyright (c) 2024 Kiv. All rights reserved.
 * 
 * 這是一個示範如何使用 SuperConnection 元件的範例程式。
 * 展示以下功能：
 * - 基本查詢操作
 * - 交易處理
 * - 參數化查詢
 * - 錯誤處理
 */

using System;
using System.Threading.Tasks;
using System.Data;
using Kiv.SuperConnection;
using Microsoft.Data.SqlClient;

namespace SuperConnection.Example
{
    /// <summary>
    /// SuperConnection 使用範例程式
    /// </summary>
    class Program
    {
        /// <summary>
        /// 程式進入點
        /// </summary>
        static async Task Main(string[] args)
        {
            try
            {
                // 建立資料庫存取物件
                using var dataAccess = new SuperConnectionAccess(
                    "Server=localhost;Database=YourDB;Trusted_Connection=True;"
                );

                // 範例 1：執行查詢並取得資料表
                Console.WriteLine("執行查詢範例...");
                var dataTable = await dataAccess.ExecuteQueryAsync(
                    "SELECT * FROM Users WHERE Age > @Age",
                    new Dictionary<string, object> { { "@Age", 18 } }
                );

                // 顯示查詢結果
                foreach (DataRow row in dataTable.Rows)
                {
                    Console.WriteLine($"使用者: {row["Name"]}, 年齡: {row["Age"]}");
                }

                // 範例 2：執行交易
                Console.WriteLine("\n執行交易範例...");
                var success = await dataAccess.ExecuteTransactionAsync(async transaction =>
                {
                    // 更新使用者資料
                    await dataAccess.ExecuteNonQueryAsync(
                        "UPDATE Users SET Age = @Age WHERE Id = @Id",
                        new Dictionary<string, object>
                        {
                            { "@Age", 25 },
                            { "@Id", 1 }
                        }
                    );

                    // 插入新記錄
                    await dataAccess.ExecuteNonQueryAsync(
                        "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)",
                        new Dictionary<string, object>
                        {
                            { "@Name", "新使用者" },
                            { "@Age", 30 }
                        }
                    );
                });

                Console.WriteLine($"交易執行結果: {(success ? "成功" : "失敗")}");

                // 範例 3：執行單一值查詢
                Console.WriteLine("\n執行單一值查詢範例...");
                var count = await dataAccess.ExecuteScalarAsync(
                    "SELECT COUNT(*) FROM Users WHERE Age > @Age",
                    new Dictionary<string, object> { { "@Age", 20 } }
                );

                Console.WriteLine($"20歲以上的使用者數量: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
            }

            Console.WriteLine("\n按任意鍵結束...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 使用者類別範例
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
} 