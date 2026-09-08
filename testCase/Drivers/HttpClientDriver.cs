using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace testCase.Drivers
{
    public class HttpClientDriver : IDisposable
    {
        private readonly HttpClient _httpClient;
        public HttpResponseMessage? LastResponse { get; private set; }
        public string? LastResponseBody { get; private set; }

        public HttpClientDriver()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string baseUrl = config["ApiBaseUrl"] ?? "http://localhost:8080";
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task SendPostAsync(string relativeUrl, Dictionary<string, string> parameters)
        {
            var content = new FormUrlEncodedContent(parameters);
            LastResponse = await _httpClient.PostAsync(relativeUrl, content);
            LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
        }

        public async Task SendPostJsonAsync(string relativeUrl, string jsonContent)
        {
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            LastResponse = await _httpClient.PostAsync(relativeUrl, content);
            LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
        }

        public async Task SendPutAsync(string relativeUrl, Dictionary<string, string> parameters)
        {
            var content = new FormUrlEncodedContent(parameters);
            LastResponse = await _httpClient.PutAsync(relativeUrl, content);
            LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
        }

        public async Task SendGetAsync(string relativeUrl)
        {
            LastResponse = await _httpClient.GetAsync(relativeUrl);
            LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
