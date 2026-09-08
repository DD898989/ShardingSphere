using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reqnroll;
using Xunit;
using testCase.Drivers;

namespace testCase.StepDefinitions
{
    [Binding]
    public class DatabaseSteps
    {
        private readonly DatabaseDriver _databaseDriver;
        private readonly ScenarioContext _scenarioContext;

        public DatabaseSteps(DatabaseDriver databaseDriver, ScenarioContext scenarioContext)
        {
            _databaseDriver = databaseDriver;
            _scenarioContext = scenarioContext;
        }

        private string ResolvePlaceholders(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            foreach (var key in _scenarioContext.Keys)
            {
                var valStr = _scenarioContext[key]?.ToString();
                if (valStr == null) continue;

                // Standard placeholder: {LastGeneratedOrderId}
                var placeholder = $"{{{key}}}";
                if (input.Contains(placeholder))
                {
                    input = input.Replace(placeholder, valStr);
                }

                // Modulo placeholder: {LastGeneratedOrderId % 2}
                var modPlaceholder = $"{{{key} % 2}}";
                if (input.Contains(modPlaceholder) && long.TryParse(valStr, out long parsedVal))
                {
                    var modVal = parsedVal % 2;
                    // In some DB/Sharding configurations, negative modulo can happen if the number is negative,
                    // but Snowflake IDs are positive. Just in case, take absolute value.
                    var absModVal = Math.Abs(modVal);
                    input = input.Replace(modPlaceholder, absModVal.ToString());
                }
            }
            return input;
        }

        [Then(@"應該在底層 MySQL 中驗證以下分片記錄符合配置:")]
        [Then(@"应该在底层 MySQL 中验证以下分片记录符合配置:")]
        public async Task ThenVerifyMultipleRecordsMeetConfig(Table table)
        {
            string dbCol = table.Header.Contains("數據庫") ? "數據庫" : "数据库";
            string tblCol = table.Header.Contains("數據表") ? "數據表" : "数据表";
            string idColCol = table.Header.Contains("主鍵欄位") ? "主鍵欄位" : (table.Header.Contains("主键字段") ? "主键字段" : "主鍵字段");
            string idValCol = table.Header.Contains("主鍵值") ? "主鍵值" : (table.Header.Contains("主键值") ? "主键值" : "主鍵值");

            // Group by (Database, Table, IdColumn, IdValue) -> Dictionary<Field, ExpectedValue>
            var groups = new Dictionary<(string Db, string Tbl, string IdCol, object IdVal), Dictionary<string, string>>();

            bool isOldFormat = table.Header.Contains("欄位") || table.Header.Contains("字段");

            if (isOldFormat)
            {
                foreach (var row in table.Rows)
                {
                    string db = ResolvePlaceholders(row[dbCol]);
                    string tbl = ResolvePlaceholders(row[tblCol]);
                    string idCol = ResolvePlaceholders(row[idColCol]);
                    string idValStr = ResolvePlaceholders(row[idValCol]);
                    string fieldCol = table.Header.Contains("欄位") ? "欄位" : "字段";
                    string expectedCol = table.Header.Contains("預期值") ? "預期值" : "预期值";
                    string field = ResolvePlaceholders(row[fieldCol]);
                    string expected = ResolvePlaceholders(row[expectedCol]);

                    object idVal = idValStr;
                    if (long.TryParse(idValStr, out long parsedLong))
                    {
                        idVal = parsedLong;
                    }

                    var key = (db, tbl, idCol, idVal);
                    if (!groups.ContainsKey(key))
                    {
                        groups[key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    groups[key][field] = expected;
                }
            }
            else
            {
                // New Format: Column names are headers
                var valueColumns = new List<string>();
                foreach (var header in table.Header)
                {
                    if (header != dbCol && header != tblCol && header != idColCol && header != idValCol &&
                        header != "主鍵欄位" && header != "主键字段" && header != "主鍵字段" &&
                        header != "主鍵值" && header != "主键值")
                    {
                        valueColumns.Add(header);
                    }
                }

                foreach (var row in table.Rows)
                {
                    string db = ResolvePlaceholders(row[dbCol]);
                    string tbl = ResolvePlaceholders(row[tblCol]);
                    string idCol = ResolvePlaceholders(row[idColCol]);
                    string idValStr = ResolvePlaceholders(row[idValCol]);

                    object idVal = idValStr;
                    if (long.TryParse(idValStr, out long parsedLong))
                    {
                        idVal = parsedLong;
                    }

                    var key = (db, tbl, idCol, idVal);
                    if (!groups.ContainsKey(key))
                    {
                        groups[key] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var col in valueColumns)
                    {
                        groups[key][col] = ResolvePlaceholders(row[col]);
                    }
                }
            }

            foreach (var kvp in groups)
            {
                string db = kvp.Key.Db;
                string tbl = kvp.Key.Tbl;
                string idCol = kvp.Key.IdCol;
                object idVal = kvp.Key.IdVal;
                var expectedValues = kvp.Value;

                bool existsAndMatches = await _databaseDriver.VerifyRecordExistsAsync(
                    db, tbl, idCol, idVal, expectedValues);

                Assert.True(existsAndMatches, $"在資料庫 {db} 的表 {tbl} 中找不到主鍵 {idCol}={idVal} 且符合預期的記錄。");
            }
        }

        [Then(@"應該在底層 MySQL 中驗證主鍵 ""(.*)"" 為 ""(.*)"" 的記錄符合以下配置:")]
        [Then(@"应该在底层 MySQL 中验证主键 ""(.*)"" 为 ""(.*)"" 的记录符合以下配置:")]
        public async Task ThenVerifyRecordMeetsShardingConfig(string idColumn, string idValuePlaceholder, Table table)
        {
            string resolvedIdColumn = ResolvePlaceholders(idColumn);
            string resolvedIdValueStr = ResolvePlaceholders(idValuePlaceholder);

            object resolvedIdValue = resolvedIdValueStr;
            if (long.TryParse(resolvedIdValueStr, out long parsedLong))
            {
                resolvedIdValue = parsedLong;
            }

            // Group validation rows by (database, table)
            // Column names can be: 數據庫/数据库, 數據表/数据表, 欄位/字段, 預期值/预期值
            var groups = new Dictionary<(string Db, string Tbl), Dictionary<string, string>>();

            foreach (var row in table.Rows)
            {
                string dbCol = table.Header.Contains("數據庫") ? "數據庫" : "数据库";
                string tblCol = table.Header.Contains("數據表") ? "數據表" : "数据表";
                string fieldCol = table.Header.Contains("欄位") ? "欄位" : "字段";
                string expectedCol = table.Header.Contains("預期值") ? "預期值" : "预期值";

                string db = ResolvePlaceholders(row[dbCol]);
                string tbl = ResolvePlaceholders(row[tblCol]);
                string field = ResolvePlaceholders(row[fieldCol]);
                string expected = ResolvePlaceholders(row[expectedCol]);

                var key = (db, tbl);
                if (!groups.ContainsKey(key))
                {
                    groups[key] = new Dictionary<string, string>();
                }
                groups[key][field] = expected;
            }

            foreach (var kvp in groups)
            {
                string db = kvp.Key.Db;
                string tbl = kvp.Key.Tbl;
                var expectedValues = kvp.Value;

                bool existsAndMatches = await _databaseDriver.VerifyRecordExistsAsync(
                    db, tbl, resolvedIdColumn, resolvedIdValue, expectedValues);

                Assert.True(existsAndMatches, $"在資料庫 {db} 的表 {tbl} 中找不到主鍵 {resolvedIdColumn}={resolvedIdValueStr} 且符合預期的記錄。");
            }
        }

        [Then(@"應該在底層的 MySQL 數據庫 ""(.*)"" 的表 ""(.*)"" 中找到主鍵 ""(.*)"" 為 ""(.*)"" 的記錄，且滿足以下欄位值:")]
        [Then(@"规律在底层的 MySQL 数据库 ""(.*)"" 的表 ""(.*)"" 中找到主键 ""(.*)"" 为 ""(.*)"" 的记录，且满足以下字段值:")]
        public async Task ThenShouldFindRecordInDatabaseAndTableWithExpectedValues(
            string database, string table, string idColumn, string idValuePlaceholder, Table expectedValuesTable)
        {
            string resolvedDatabase = ResolvePlaceholders(database);
            string resolvedTable = ResolvePlaceholders(table);
            string resolvedIdColumn = ResolvePlaceholders(idColumn);
            string resolvedIdValueStr = ResolvePlaceholders(idValuePlaceholder);

            // Convert idValueStr to appropriate type (long if it's a dynamic Snowflake ID)
            object resolvedIdValue = resolvedIdValueStr;
            if (long.TryParse(resolvedIdValueStr, out long parsedLong))
            {
                resolvedIdValue = parsedLong;
            }

            var expectedValues = new Dictionary<string, string>();
            foreach (var row in expectedValuesTable.Rows)
            {
                string key = "";
                string value = "";

                if (expectedValuesTable.Header.Contains("欄位") && expectedValuesTable.Header.Contains("預期值"))
                {
                    key = row["欄位"];
                    value = row["預期值"];
                }
                else if (expectedValuesTable.Header.Contains("字段") && expectedValuesTable.Header.Contains("预期值"))
                {
                    key = row["字段"];
                    value = row["预期值"];
                }
                else if (expectedValuesTable.Header.Contains("欄位") && expectedValuesTable.Header.Contains("值"))
                {
                    key = row["欄位"];
                    value = row["值"];
                }
                else if (expectedValuesTable.Header.Contains("字段") && expectedValuesTable.Header.Contains("值"))
                {
                    key = row["字段"];
                    value = row["值"];
                }

                if (!string.IsNullOrEmpty(key))
                {
                    expectedValues[key] = ResolvePlaceholders(value);
                }
            }

            if (expectedValues.Count == 0 && expectedValuesTable.Rows.Count > 0)
            {
                foreach (var header in expectedValuesTable.Header)
                {
                    expectedValues[header] = ResolvePlaceholders(expectedValuesTable.Rows[0][header]);
                }
            }

            bool existsAndMatches = await _databaseDriver.VerifyRecordExistsAsync(
                resolvedDatabase, resolvedTable, resolvedIdColumn, resolvedIdValue, expectedValues);

            Assert.True(existsAndMatches, $"在資料庫 {resolvedDatabase} 的表 {resolvedTable} 中找不到主鍵 {resolvedIdColumn}={resolvedIdValueStr} 且符合預期的記錄。");
        }

        [Then(@"應該在底層的 MySQL 數據庫 ""(.*)"" 的表 ""(.*)"" 中找不到主鍵 ""(.*)"" 為 ""(.*)"" 的記錄")]
        [Then(@"应该在底层的 MySQL 数据库 ""(.*)"" 的表 ""(.*)"" 中找不到主键 ""(.*)"" 为 ""(.*)"" 的记录")]
        public async Task ThenShouldNotFindRecordInDatabaseAndTable(string database, string table, string idColumn, string idValuePlaceholder)
        {
            string resolvedDatabase = ResolvePlaceholders(database);
            string resolvedTable = ResolvePlaceholders(table);
            string resolvedIdColumn = ResolvePlaceholders(idColumn);
            string resolvedIdValueStr = ResolvePlaceholders(idValuePlaceholder);

            object resolvedIdValue = resolvedIdValueStr;
            if (long.TryParse(resolvedIdValueStr, out long parsedLong))
            {
                resolvedIdValue = parsedLong;
            }

            var record = await _databaseDriver.QueryRecordAsync(resolvedDatabase, resolvedTable, resolvedIdColumn, resolvedIdValue);
            Assert.Null(record);
        }
    }
}
