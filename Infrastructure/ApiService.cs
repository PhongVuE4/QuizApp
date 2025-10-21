using Core.Models;
using Core.Models.DTOs;
using Core.Models.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public ApiService()
        {
            // Đọc appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _baseUrl = config["ApiSettings:BaseUrl"];

            if (string.IsNullOrEmpty(_baseUrl))
                throw new InvalidOperationException("⚠️ BaseUrl not found in appsettings.json");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        private async Task<List<T>> GetDataAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(json);
            return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        //public Task<List<Question>> GetQuestionsAsync() => GetDataAsync<Question>("question/questions");
        public async Task<List<Question>> GetQuestionsAsync()
        {
            var response = await _httpClient.GetAsync("question/questions");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Lỗi API: {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync();

            // ✅ Parse JSON thành List<Question>
            var result = JsonSerializer.Deserialize<List<Question>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new List<Question>();
        }

        public Task<List<Subject>> GetSubjectsAsync() => GetDataAsync<Subject>("subject/subjects");
        public Task<List<Class>> GetClassesAsync() => GetDataAsync<Class>("class");

        public async Task<List<Question>> GetQuestionByClassAndSubject(string? classId = null, string? subjectId = null)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(classId))
                queryParams.Add($"classId={classId}");
            if (!string.IsNullOrEmpty(subjectId))
                queryParams.Add($"subjectId={subjectId}");

            string query = string.Join("&", queryParams);
            string endpoint = string.IsNullOrEmpty(query) ? "question/filter" : $"question/filter?{query}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json); // 👉 In ra để debug xem JSON

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<Question>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Data ?? new List<Question>();
        }


        //public async Task<List<Question>> GetQuestionByClassandSubject(string? classId = null, string? subjectId = null)
        //{
        //    var queryParams = new List<string>();

        //    if (!string.IsNullOrEmpty(classId))
        //        queryParams.Add($"classId={classId}");
        //    if (!string.IsNullOrEmpty(subjectId))
        //        queryParams.Add($"subjectId={subjectId}");

        //    string query = string.Join("&", queryParams);
        //    string endpoint = string.IsNullOrEmpty(query) ? "question/filter" : $"question/filter?{query}";

        //    return await GetDataAsync<Question>(endpoint);
        //}

    }
}
