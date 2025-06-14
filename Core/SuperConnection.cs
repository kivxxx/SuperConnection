/*
 * SuperConnection
 * Copyright (c) 2024 Kiv. All rights reserved.
 * 
 * 這是一個高效能的 SQL Server 連線池管理元件，提供自動化的連線管理和資源優化。
 * 主要功能包括：
 * - 自動連線池管理
 * - 非同步操作支援
 * - 資源自動回收
 * - 連線狀態監控
 */

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Kiv.SuperConnection
{
    /// <summary>
    /// 資料庫連線管理類別，提供連線池管理和資源優化功能
    /// </summary>
    public sealed class SuperConnection : IDisposable, IAsyncDisposable
    {
        // 連線字串
        private readonly string _connectionString;
        
        // 連線池，使用 ConcurrentQueue 確保執行緒安全
        private readonly ConcurrentQueue<SqlConnection> _connectionPool;
        
        // 信號量，用於控制並發連線數量
        private readonly SemaphoreSlim _semaphore;
        
        // 連線池配置
        private readonly int _maxPoolSize;    // 最大連線數
        private readonly int _minPoolSize;    // 最小連線數
        
        // 維護計時器，定期檢查連線池狀態
        private readonly Timer _maintenanceTimer;
        
        // 資源釋放標記
        private bool _disposed;
        
        // 同步鎖定物件
        private readonly object _lockObject = new();

        /// <summary>
        /// 初始化連線管理類別
        /// </summary>
        /// <param name="connectionString">資料庫連線字串</param>
        /// <param name="maxPoolSize">最大連線池大小，預設為 100</param>
        /// <param name="minPoolSize">最小連線池大小，預設為 5</param>
        /// <exception cref="ArgumentNullException">當連線字串為 null 時拋出</exception>
        public SuperConnection(string connectionString, int maxPoolSize = 100, int minPoolSize = 5)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _maxPoolSize = maxPoolSize;
            _minPoolSize = minPoolSize;
            _connectionPool = new ConcurrentQueue<SqlConnection>();
            _semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);

            // 初始化連線池
            InitializeConnectionPool();

            // 啟動維護計時器，每 5 分鐘執行一次維護
            _maintenanceTimer = new Timer(
                MaintainConnectionPool,
                null,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(5)
            );
        }

        /// <summary>
        /// 初始化連線池，建立最小數量的連線
        /// </summary>
        private void InitializeConnectionPool()
        {
            for (int i = 0; i < _minPoolSize; i++)
            {
                _ = CreateAndAddConnection();
            }
        }

        /// <summary>
        /// 維護連線池，確保連線可用性和效能
        /// </summary>
        /// <param name="state">計時器狀態參數</param>
        private async void MaintainConnectionPool(object? state)
        {
            try
            {
                // 確保最小連線數
                while (_connectionPool.Count < _minPoolSize)
                {
                    await CreateAndAddConnection();
                }

                // 檢查並關閉無效連線
                var validConnections = new ConcurrentQueue<SqlConnection>();
                while (_connectionPool.TryDequeue(out var connection))
                {
                    try
                    {
                        if (connection.State == System.Data.ConnectionState.Closed)
                        {
                            await connection.OpenAsync();
                        }
                        validConnections.Enqueue(connection);
                    }
                    catch
                    {
                        connection.Dispose();
                    }
                }

                // 更新連線池
                _connectionPool.Clear();
                foreach (var connection in validConnections)
                {
                    _connectionPool.Enqueue(connection);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"維護連線池時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 建立並加入新連線到連線池
        /// </summary>
        /// <returns>非同步操作任務</returns>
        private async Task CreateAndAddConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                _connectionPool.Enqueue(connection);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"建立新連線時發生錯誤: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 從連線池取得可用連線
        /// </summary>
        /// <returns>可用的資料庫連線</returns>
        /// <exception cref="ObjectDisposedException">當物件已被釋放時拋出</exception>
        public async Task<SqlConnection> GetConnectionAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SuperConnection));

            await _semaphore.WaitAsync();

            try
            {
                if (_connectionPool.TryDequeue(out var connection))
                {
                    try
                    {
                        if (connection.State == System.Data.ConnectionState.Closed)
                        {
                            await connection.OpenAsync();
                        }
                        return connection;
                    }
                    catch
                    {
                        connection.Dispose();
                        return await CreateNewConnectionAsync();
                    }
                }

                return await CreateNewConnectionAsync();
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        /// <summary>
        /// 建立新的資料庫連線
        /// </summary>
        /// <returns>新建立的資料庫連線</returns>
        private async Task<SqlConnection> CreateNewConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// 釋放連線回連線池
        /// </summary>
        /// <param name="connection">要釋放的連線</param>
        /// <exception cref="ArgumentNullException">當連線為 null 時拋出</exception>
        public void ReleaseConnection(SqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (_disposed)
            {
                connection.Dispose();
                return;
            }

            try
            {
                if (_connectionPool.Count < _maxPoolSize)
                {
                    _connectionPool.Enqueue(connection);
                }
                else
                {
                    connection.Dispose();
                }
            }
            finally
            {
                _semaphore.Release();
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
            _maintenanceTimer.Dispose();

            while (_connectionPool.TryDequeue(out var connection))
            {
                await connection.DisposeAsync();
            }

            _semaphore.Dispose();
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
} 