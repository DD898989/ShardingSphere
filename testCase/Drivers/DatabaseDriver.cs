using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace testCase.Drivers
{
    public class DatabaseDriver
    {
        private readonly string _connectionStringTemplate;

        public DatabaseDriver()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionStringTemplate = config["DbConnectionStringTemplate"] 
                ?? "Server=127.0.0.1;Port=3306;User ID=root;Password=;Database={0};AllowPublicKeyRetrieval=true;SSL Mode=None;";
        }

        private string GetConnectionString(string database)
        {
            return string.Format(_connectionStringTemplate, database);
        }

        public async Task<Dictionary<string, object>?> QueryRecordAsync(string database, string table, string idColumn, object idValue)
        {
            string connectionString = GetConnectionString(database);
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = $"SELECT * FROM `{table}` WHERE `{idColumn}` = @id LIMIT 1";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", idValue);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                return row;
            }

            return null;
        }

        public async Task<bool> VerifyRecordExistsAsync(string database, string table, string idColumn, object idValue, Dictionary<string, string> expectedValues)
        {
            var record = await QueryRecordAsync(database, table, idColumn, idValue);
            if (record == null)
            {
                return false;
            }

            foreach (var kvp in expectedValues)
            {
                string columnName = kvp.Key;
                string expectedValStr = kvp.Value;

                if (!record.TryGetValue(columnName, out object? actualVal))
                {
                    throw new Exception($"在資料庫 {database} 的表 {table} 中找不到欄位 '{columnName}'");
                }

                string actualValStr = actualVal switch
                {
                    null => "null",
                    DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => actualVal.ToString()!
                };

                // For comparing date strings or standard string representation, tolerate minor formatting differences
                if (actualVal is DateTime dtValue && DateTime.TryParse(expectedValStr, out DateTime expectedDtValue))
                {
                    if (dtValue != expectedDtValue)
                    {
                        throw new Exception($"欄位 '{columnName}' 驗證失敗。預期: {expectedDtValue:yyyy-MM-dd HH:mm:ss}, 實際: {dtValue:yyyy-MM-dd HH:mm:ss}");
                    }
                }
                else if (!string.Equals(actualValStr.Trim(), expectedValStr.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"欄位 '{columnName}' 驗證失敗。預期: '{expectedValStr}', 實際: '{actualValStr}'");
                }
            }

            return true;
        }
    }
}
