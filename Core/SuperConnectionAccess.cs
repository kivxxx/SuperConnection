/*
 * SuperConnection
 * Copyright (c) 2024 Kiv. All rights reserved.
 * 
 * 這是一個高效能的資料庫存取元件，提供完整的資料庫操作功能。
 * 主要功能包括：
 * - 資料查詢和命令執行
 * - 交易管理
 * - 參數化查詢
 * - 錯誤處理
 */

using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Kiv.SuperConnection
{
    /// <summary>
    /// 資料庫存取類別，提供完整的資料庫操作功能
    /// </summary>
    public class SuperConnectionAccess : IDisposable, IAsyncDisposable
    {
        // 連線管理物件
        private readonly SuperConnection? _connectionManager;
        private readonly string _connectionString;
        private readonly bool _useConnectionPool;
        
        // 資源釋放標記
        private bool _disposed;

        /// <summary>
        /// 初始化資料庫存取類別（使用連線池）
        /// </summary>
        /// <param name="connectionString">資料庫連線字串</param>
        /// <param name="maxPoolSize">最大連線池大小</param>
        /// <param name="minPoolSize">最小連線池大小</param>
        public SuperConnectionAccess(string connectionString, int maxPoolSize = 100, int minPoolSize = 5)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _connectionManager = new SuperConnection(connectionString, maxPoolSize, minPoolSize);
            _useConnectionPool = true;
        }

        /// <summary>
        /// 初始化資料庫存取類別（不使用連線池）
        /// </summary>
        /// <param name="connectionString">資料庫連線字串</param>
        /// <param name="useConnectionPool">是否使用連線池，設為 false 時每次查詢都會建立新連線</param>
        public SuperConnectionAccess(string connectionString, bool useConnectionPool)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _useConnectionPool = useConnectionPool;
            if (useConnectionPool)
            {
                _connectionManager = new SuperConnection(connectionString);
            }
        }

        /// <summary>
        /// 執行查詢並返回資料表
        /// </summary>
        /// <param name="query">SQL 查詢語句</param>
        /// <param name="parameters">查詢參數</param>
        /// <returns>查詢結果資料表</returns>
        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            if (_useConnectionPool)
            {
                using var connection = await _connectionManager!.GetConnectionAsync();
                using var command = new SqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                var dataTable = new DataTable();
                using var adapter = new SqlDataAdapter(command);
                await Task.Run(() => adapter.Fill(dataTable));
                return dataTable;
            }
            else
            {
                using var connection = new SqlConnection(_connectionString);
                try
                {
                    await connection.OpenAsync();
                    using var command = new SqlCommand(query, connection);
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    var dataTable = new DataTable();
                    using var adapter = new SqlDataAdapter(command);
                    await Task.Run(() => adapter.Fill(dataTable));
                    return dataTable;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// 執行非查詢命令（如 INSERT、UPDATE、DELETE）
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="parameters">命令參數</param>
        /// <returns>受影響的資料列數</returns>
        public async Task<int> ExecuteNonQueryAsync(string commandText, Dictionary<string, object>? parameters = null)
        {
            if (_useConnectionPool)
            {
                using var connection = await _connectionManager!.GetConnectionAsync();
                using var command = new SqlCommand(commandText, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            else
            {
                using var connection = new SqlConnection(_connectionString);
                try
                {
                    await connection.OpenAsync();
                    using var command = new SqlCommand(commandText, connection);
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    return await command.ExecuteNonQueryAsync();
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// 執行查詢並返回單一值
        /// </summary>
        /// <param name="query">SQL 查詢語句</param>
        /// <param name="parameters">查詢參數</param>
        /// <returns>查詢結果的第一個欄位值</returns>
        public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object>? parameters = null)
        {
            if (_useConnectionPool)
            {
                using var connection = await _connectionManager!.GetConnectionAsync();
                using var command = new SqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteScalarAsync();
            }
            else
            {
                using var connection = new SqlConnection(_connectionString);
                try
                {
                    await connection.OpenAsync();
                    using var command = new SqlCommand(query, connection);
                    
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    return await command.ExecuteScalarAsync();
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// 執行交易
        /// </summary>
        /// <param name="transactionAction">交易動作委派</param>
        /// <returns>交易是否成功</returns>
        public async Task<bool> ExecuteTransactionAsync(Func<SqlTransaction, Task> transactionAction)
        {
            if (!_useConnectionPool)
            {
                throw new InvalidOperationException("交易操作必須使用連線池模式");
            }

            using var connection = await _connectionManager!.GetConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await transactionAction(transaction);
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 執行查詢並返回資料表，查詢完成後自動斷開連線
        /// </summary>
        /// <param name="query">SQL 查詢語句</param>
        /// <param name="parameters">查詢參數</param>
        /// <returns>查詢結果資料表</returns>
        public async Task<DataTable> ExecuteQueryWithDisconnectAsync(string query, Dictionary<string, object>? parameters = null)
        {
            var connection = await _connectionManager.GetConnectionAsync();
            try
            {
                using var command = new SqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                var dataTable = new DataTable();
                using var adapter = new SqlDataAdapter(command);
                await Task.Run(() => adapter.Fill(dataTable));
                return dataTable;
            }
            finally
            {
                await connection.CloseAsync();
                connection.Dispose();
            }
        }

        /// <summary>
        /// 非同步釋放資源
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            if (_connectionManager != null)
            {
                await _connectionManager.DisposeAsync();
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// 資料庫存取類別，提供每次查詢後自動斷開連線的功能
    /// </summary>
    public class SuperConnectionDisposableAccess
    {
        private readonly string _connectionString;

        /// <summary>
        /// 初始化資料庫存取類別
        /// </summary>
        /// <param name="connectionString">資料庫連線字串</param>
        public SuperConnectionDisposableAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// 執行查詢並返回資料表，查詢完成後自動斷開連線
        /// </summary>
        /// <param name="query">SQL 查詢語句</param>
        /// <param name="parameters">查詢參數</param>
        /// <returns>查詢結果資料表</returns>
        public async Task<DataTable> ExecuteQueryWithDisconnectAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                using var command = new SqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                var dataTable = new DataTable();
                using var adapter = new SqlDataAdapter(command);
                await Task.Run(() => adapter.Fill(dataTable));
                return dataTable;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        /// <summary>
        /// 執行非查詢命令（如 INSERT、UPDATE、DELETE），執行完成後自動斷開連線
        /// </summary>
        /// <param name="commandText">SQL 命令</param>
        /// <param name="parameters">命令參數</param>
        /// <returns>受影響的資料列數</returns>
        public async Task<int> ExecuteNonQueryWithDisconnectAsync(string commandText, Dictionary<string, object>? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                using var command = new SqlCommand(commandText, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        /// <summary>
        /// 執行查詢並返回單一值，查詢完成後自動斷開連線
        /// </summary>
        /// <param name="query">SQL 查詢語句</param>
        /// <param name="parameters">查詢參數</param>
        /// <returns>查詢結果的第一個欄位值</returns>
        public async Task<object?> ExecuteScalarWithDisconnectAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                using var command = new SqlCommand(query, connection);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return await command.ExecuteScalarAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
    }
} 