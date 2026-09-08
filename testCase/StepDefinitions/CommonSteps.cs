using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Reqnroll;
using Xunit;
using testCase.Drivers;

namespace testCase.StepDefinitions
{
    [Binding]
    public class CommonSteps
    {
        private readonly HttpClientDriver _httpClientDriver;
        private readonly ScenarioContext _scenarioContext;

        public CommonSteps(HttpClientDriver httpClientDriver, ScenarioContext scenarioContext)
        {
            _httpClientDriver = httpClientDriver;
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
                    var absModVal = Math.Abs(modVal);
                    input = input.Replace(modPlaceholder, absModVal.ToString());
                }
            }
            return input;
        }

        [When(@"我發送 POST 請求至 ""(.*)"" 帶有以下 JSON 內容:")]
        [When(@"我发送 POST 请求到 ""(.*)"" 带有以下 JSON 内容:")]
        public async Task WhenISendPostRequestWithJsonContent(string relativeUrl, string jsonContent)
        {
            string resolvedJsonContent = ResolvePlaceholders(jsonContent);
            await _httpClientDriver.SendPostJsonAsync(relativeUrl, resolvedJsonContent);
        }

        [When(@"我發送 POST 請求至 ""(.*)"" 帶有以下參數:")]
        [When(@"我发送 POST 请求到 ""(.*)"" 带有以下参数:")]
        public async Task WhenISendPostRequestWithParameters(string relativeUrl, Table table)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var row in table.Rows)
            {
                // In Reqnroll, tables typically have headers. For parameters, we support two table styles:
                // Style 1: Key-Value pairs (header: "欄位" / "Key", "值" / "Value")
                // Style 2: Single-row table where headers are keys and values are in row 0
                if (table.Header.Contains("Key") && table.Header.Contains("Value"))
                {
                    parameters[row["Key"]] = ResolvePlaceholders(row["Value"]);
                }
                else if (table.Header.Contains("欄位") && table.Header.Contains("值"))
                {
                    parameters[row["欄位"]] = ResolvePlaceholders(row["值"]);
                }
                else if (table.Header.Contains("参数") && table.Header.Contains("值"))
                {
                    parameters[row["参数"]] = ResolvePlaceholders(row["值"]);
                }
            }

            // If it's Style 2 (one header row = keys, and row[0] = values)
            if (parameters.Count == 0 && table.Rows.Count > 0)
            {
                foreach (var header in table.Header)
                {
                    parameters[header] = ResolvePlaceholders(table.Rows[0][header]);
                }
            }

            await _httpClientDriver.SendPostAsync(relativeUrl, parameters);
        }

        [When(@"我發送 PUT 請求至 ""(.*)"" 帶有以下參數:")]
        [When(@"我发送 PUT 请求到 ""(.*)"" 带有以下参数:")]
        public async Task WhenISendPutRequestWithParameters(string relativeUrl, Table table)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var row in table.Rows)
            {
                if (table.Header.Contains("Key") && table.Header.Contains("Value"))
                {
                    parameters[row["Key"]] = ResolvePlaceholders(row["Value"]);
                }
                else if (table.Header.Contains("欄位") && table.Header.Contains("值"))
                {
                    parameters[row["欄位"]] = ResolvePlaceholders(row["值"]);
                }
                else if (table.Header.Contains("参数") && table.Header.Contains("值"))
                {
                    parameters[row["参数"]] = ResolvePlaceholders(row["值"]);
                }
            }

            if (parameters.Count == 0 && table.Rows.Count > 0)
            {
                foreach (var header in table.Header)
                {
                    parameters[header] = ResolvePlaceholders(table.Rows[0][header]);
                }
            }

            // Resolve placeholders in the URL as well
            string resolvedUrl = ResolvePlaceholders(relativeUrl);
            await _httpClientDriver.SendPutAsync(resolvedUrl, parameters);
        }

        [When(@"我發送 GET 請求至 ""(.*)""")]
        [When(@"我发送 GET 请求到 ""(.*)""")]
        public async Task WhenISendGetRequest(string relativeUrl)
        {
            string resolvedUrl = ResolvePlaceholders(relativeUrl);
            await _httpClientDriver.SendGetAsync(resolvedUrl);
        }

        [Then(@"響應狀態碼應為 (.*)")]
        [Then(@"响应状态码应为 (.*)")]
        public void ThenResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.NotNull(_httpClientDriver.LastResponse);
            int actualStatusCode = (int)_httpClientDriver.LastResponse.StatusCode;
            Assert.Equal(expectedStatusCode, actualStatusCode);
        }

        [Then(@"響應的 JSON 內容應包含以下欄位與值:")]
        [Then(@"响应的 JSON 内容应包含以下字段与值:")]
        public void ThenResponseJsonShouldContain(Table table)
        {
            Assert.NotNull(_httpClientDriver.LastResponseBody);
            using var doc = JsonDocument.Parse(_httpClientDriver.LastResponseBody);
            var root = doc.RootElement;

            // Handle two styles of verification tables:
            // Style 1: Key-Value columns
            // Style 2: Header-Row structure
            var expectedValues = new Dictionary<string, string>();
            foreach (var row in table.Rows)
            {
                if (table.Header.Contains("欄位") && table.Header.Contains("值"))
                {
                    expectedValues[row["欄位"]] = ResolvePlaceholders(row["值"]);
                }
                else if (table.Header.Contains("字段") && table.Header.Contains("值"))
                {
                    expectedValues[row["字段"]] = ResolvePlaceholders(row["值"]);
                }
            }

            if (expectedValues.Count == 0 && table.Rows.Count > 0)
            {
                foreach (var header in table.Header)
                {
                    expectedValues[header] = ResolvePlaceholders(table.Rows[0][header]);
                }
            }

            foreach (var kvp in expectedValues)
            {
                string propertyName = kvp.Key;
                string expectedValue = kvp.Value;

                // Try to find property in JSON (JSON properties in C# are usually camelCase from Spring Boot,
                // but let's do a case-insensitive match)
                bool found = false;
                foreach (var property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        string actualValue = property.Value.ValueKind switch
                        {
                            JsonValueKind.Null => "null",
                            _ => property.Value.ToString()
                        };

                        if (DateTime.TryParse(actualValue, out DateTime actualDt) && DateTime.TryParse(expectedValue, out DateTime expectedDt))
                        {
                            Assert.Equal(expectedDt, actualDt);
                        }
                        else
                        {
                            Assert.Equal(expectedValue.Trim(), actualValue.Trim());
                        }
                        break;
                    }
                }

                Assert.True(found, $"找不到響應 JSON 中的欄位 '{propertyName}'。完整的 Response: {_httpClientDriver.LastResponseBody}");
            }
        }

        [Then(@"解析響應並將欄位 ""(.*)"" 儲存為 ""(.*)""")]
        [Then(@"解析响应并将字段 ""(.*)"" 储存为 ""(.*)""")]
        public void ThenParseResponseAndStoreProperty(string propertyName, string contextKey)
        {
            Assert.NotNull(_httpClientDriver.LastResponseBody);
            using var doc = JsonDocument.Parse(_httpClientDriver.LastResponseBody);
            var root = doc.RootElement;

            bool found = false;
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    string value = property.Value.ToString();
                    _scenarioContext[contextKey] = value;
                    break;
                }
            }

            Assert.True(found, $"無法解析響應 JSON，找不到欄位 '{propertyName}'。完整的 Response: {_httpClientDriver.LastResponseBody}");
        }
    }
}
